using Microsoft.EntityFrameworkCore;

// Adaptador de transição: os endpoints continuam usando suas coleções, mas
// cada requisição carrega dados do MySQL e grava as alterações em uma transação.
// A instância é Scoped: nunca compartilhada entre requisições.
partial class Store
{
    readonly ClickClubDbContext context;

    public Store(ClickClubDbContext context)
    {
        this.context = context;
        LoadMap(Users, x => x.Id);
        LoadMap(Clubs, x => x.Id);
        LoadMap(Categorias, x => x.name);
        LoadMap(Applications, x => x.Id);
        LoadMap(Members, x => x.Id);
        LoadMap(Spaces, x => x.Id);
        LoadMap(Bookings, x => x.Id);
        LoadMap(Invoices, x => x.Id);
        LoadMap(Turmas, x => x.Id);
        LoadMap(Credenciais, x => x.Id);
        LoadMap(Autorizacoes, x => x.Id);
        LoadList(Notices);
        LoadList(Tickets);
        LoadList(Events);
        LoadList(Inscricoes);
        LoadList(Acessos);
        LoadList(Itens);
        LoadList(Creditos);
        LoadList(Interdicoes);
        LoadList(Horarios);
        LoadList(Visitas);
        LoadList(InscricoesEventos);
        LoadList(Atividades);
        LoadList(Aulas);
        LoadList(Audit);
        foreach (var s in context.Set<SessaoPersistida>().ToList())
            Sessions[s.Token] = new Session(s.UserId, s.Expires);
        foreach (var c in context.Set<ListaConvidados>().ToList())
            Convidados[c.ReservaId] = c.Convidados;
        foreach (var c in context.Set<CreditoAplicado>().ToList())
            CreditosAplicados.Add(c.CreditoId);
    }

    void LoadMap<TKey, T>(Dictionary<TKey, T> target, Func<T, TKey> key) where TKey : notnull where T : class
    {
        foreach (var item in context.Set<T>().ToList()) target.Add(key(item), item);
    }

    void LoadList<T>(List<T> target) where T : class => target.AddRange(context.Set<T>().ToList());

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        Synchronize(Users.Values);
        Synchronize(Clubs.Values);
        Synchronize(Categorias.Values);
        Synchronize(Applications.Values);
        Synchronize(Members.Values);
        Synchronize(Spaces.Values);
        Synchronize(Bookings.Values);
        Synchronize(Invoices.Values);
        Synchronize(Turmas.Values);
        Synchronize(Credenciais.Values);
        Synchronize(Autorizacoes.Values);
        Synchronize(Notices);
        Synchronize(Tickets);
        Synchronize(Events);
        Synchronize(Inscricoes);
        Synchronize(Acessos);
        Synchronize(Itens);
        Synchronize(Creditos);
        Synchronize(Interdicoes);
        Synchronize(Horarios);
        Synchronize(Visitas);
        Synchronize(InscricoesEventos);
        Synchronize(Atividades);
        Synchronize(Aulas);
        Synchronize(Audit);
        Synchronize(Sessions.Select(x => new SessaoPersistida(x.Key, x.Value.UserId, x.Value.Expires)));
        Synchronize(Convidados.Select(x => new ListaConvidados(x.Key, x.Value)));
        Synchronize(CreditosAplicados.Select(x => new CreditoAplicado(x)));
        await context.SaveChangesAsync(cancellationToken);
    }

    void Synchronize<T>(IEnumerable<T> values) where T : class
    {
        var keyProperties = context.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties;
        object[] Key(T value) => keyProperties.Select(p => p.PropertyInfo!.GetValue(value)!).ToArray();
        var desired = values.ToDictionary(Key, value => value, new EntityKeyComparer());
        var tracked = context.Set<T>().Local.ToDictionary(Key, value => value, new EntityKeyComparer());
        foreach (var old in tracked)
            if (!desired.ContainsKey(old.Key)) context.Remove(old.Value);
        foreach (var item in desired)
        {
            if (!tracked.TryGetValue(item.Key, out var old)) context.Add(item.Value);
            else if (!ReferenceEquals(item.Value, old)) context.Entry(old).CurrentValues.SetValues(item.Value);
        }
    }

    sealed class EntityKeyComparer : IEqualityComparer<object[]>
    {
        public bool Equals(object[]? x, object[]? y) => x is not null && y is not null && x.SequenceEqual(y);
        public int GetHashCode(object[] key)
        {
            var hash = new HashCode();
            foreach (var part in key) hash.Add(part);
            return hash.ToHashCode();
        }
    }
}
