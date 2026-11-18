using System.Text.Json.Nodes;
namespace ClickClub.Web.Models;
public record Option(string Value,string Text);
public record Field(string Name,string Label,string Type="text",string? Value=null,bool Required=false,List<Option>? Options=null,string? Hint=null);
public record ActionForm(string Operation,string Label,List<Field>? Fields=null,string? Id=null,string? Confirm=null,string Style="primary",Dictionary<string,string>? Hidden=null);
public record Cell(string Text,string? Tone=null);
public record Row(List<Cell> Cells,List<ActionForm>? Actions=null,string? Link=null,string? LinkLabel=null);
public record Table(List<string> Headers,List<Row> Rows);
public record Section(string Title,string? Description=null,List<ActionForm>? Forms=null,Table? Table=null);
public record Stat(string Label,string Value,string Note,string Tone="teal");
public sealed class PortalModel
{
 public string Page{get;set;}="inicio";public string Title{get;set;}="Visão geral";public string Description{get;set;}="Seu clube em um só lugar.";
 public List<Stat> Stats{get;}=new();public List<Section> Sections{get;}=new();public string? QrSvg{get;set;}public string? CardName{get;set;}public string? CardExpiry{get;set;}public string? CardCredential{get;set;}public string? SelectedMonth{get;set;}
 public List<JsonNode> CalendarEvents{get;}=new();public DateTime? CalendarMonth{get;set;}public string? CalendarSpace{get;set;}public string CalendarTimeZone{get;set;}="America/Sao_Paulo";
 public static string Text(JsonNode? n,string key,string fallback="—")=>n?[key]?.ToString() is string s&&!string.IsNullOrWhiteSpace(s)?s:fallback;
}
