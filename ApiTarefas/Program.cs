using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI();
}

//isso � uma minimal API
//metodo GET com o endPoint com diret�rio  na raiz do projeto "/"
//() => "ola mundo" significa uma fun��o lambda, sem nome
//app.MapGet("/", () => "Ol� mundo");

//EndPoint com diretorio para tarefas, metodo assincrono com inje��o de
//dependencia AppDbContext, retornando uma lista de tarefas
app.MapGet("/tarefas", async (AppDbContext db) =>
    await db.Tarefas.ToListAsync());


//FindAsync() encontra a tarefa pelo ID
//caso seja uma tarefa, retornar� a tarefa
//caso o contr�rio, retornar� um NotFound
app.MapGet("/tarefas/{id}", async (int id, AppDbContext db) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ?
      Results.Ok(tarefa) : Results.NotFound("Tarefa n�o encontrada"));


//retornar� somente as tarefas com concluida = true
app.MapGet("/tarefas/concluida", async (AppDbContext db) =>
    await db.Tarefas.Where(t => t.IsConcluida == true).ToListAsync());


app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();

    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);

});


app.MapPut("/tarefas{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null)
    {
       return Results.NotFound("N�o encontrada");
    }

    //os campos Nome e IsConcluida v�o ser alterados para os campos
    //que passar no inputTarefa
    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();
    return  Results.Ok(inputTarefa);


});


app.MapDelete("/tarefas{id}", async (int id, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if (tarefa is null)
    {
        return Results.NotFound("N�o encontrada");
    }

    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();

    return Results.Ok(tarefa);

});


app.Run();


//criando a class Tarefa
//como se tivesse criando uma Class fora na pasta Models
class Tarefa
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public bool IsConcluida { get; set; }

}

class AppDbContext : DbContext
{   
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();


}


