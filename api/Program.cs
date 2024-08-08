using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var OrderFunctions = new Dictionary<string, dynamic> {
    { "number", (Expression<Func<Pokemon, int>>)(x => x.Number)},
    { "name", (Expression<Func<Pokemon, string>>)(x => x.Name)},
    { "generation", (Expression<Func<Pokemon, string>>)(x => x.Generation)},
    { "height", (Expression<Func<Pokemon, int>>)(x => x.Height)},
    { "weight", (Expression<Func<Pokemon, int>>)(x => x.Weight)},
    { "moves", (Expression<Func<Pokemon, int>>)(x => x.Moves.Count())}, // This works, but it's slow

    // These don't work. Not entirely sure why, but don't want to spend too much time on it
    // { "type1", (Expression<Func<Pokemon, string>>)(x => x.Types.ElementAt(0).Name)},
    // { "type2", (Expression<Func<Pokemon, string>>)(x => x.Types.Count > 1 ? x.Types.ElementAt(1).Name : "" )},
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddDbContext<PokemonDb>(opt => opt.UseInMemoryDatabase("PokemonList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<PokemonDb>();
    context.Database.EnsureCreated();
}

app.MapGet("/pokemon", async (
    [FromQuery(Name = "page")] int page,
    [FromQuery(Name = "page-size")] int pageSize,
    [FromQuery(Name = "number")] int? number,
    [FromQuery(Name = "name")] string? name,
    [FromQuery(Name = "type")] string? type,
    [FromQuery(Name = "generation")] string? generation,
    [FromQuery(Name = "sort")] string? sort,
    [FromQuery(Name = "sortDir")] string? sortDir,
    [FromQuery(Name = "move")] string? move,
    [FromServices] PokemonDb db) => {
        if (page < 0) {
            page = 0;
        }
        if (pageSize < 0 || pageSize > 100) {
            pageSize = 25;
        }
        sort ??= "number";
        bool sortDesc = false;
        if (sortDir == "desc") {
            sortDesc = true;
        }
        IQueryable<Pokemon> data = db.PokemonData
            .Include(d => d.Types)
            .Include(d => d.Moves);

        if (number != null) {
            data = data.Where(d => d.Number == number);
        }
        if (name != null) {
            data = data.Where(d => d.Name.Contains(name));
        }
        if (type != null) {
            data = data.Where(d => d.Types.Any(t => t.Name == type));
        }
        if (generation != null) {
            data = data.Where(d => d.Generation == generation);
        }
        if (move != null) {
            data = data.Where(d => d.Moves.Any(m => m.Name.Contains(move)));
        }

        var sortFn = OrderFunctions.TryGetValue(sort, out dynamic? value) ? value : OrderFunctions["number"];
        data = sortDesc ? Queryable.OrderByDescending(data, sortFn) : Queryable.OrderBy(data, sortFn);

        var total = data.Count();
        var results = await data
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new { total, results };
});

app.MapGet("/pokemon/{id}", async (
    [FromRoute] int id,
    [FromServices] PokemonDb db) => {
        return await db.PokemonData
            .Where(p => p.Number == id)
            .Include(d => d.Types)
            .Include(d => d.Moves)
            .Include(d => d.Abilities)
            .Include(d => d.Stats)
            .Include(d => d.PreviousEvolution)
            .Include(d => d.FutureEvolutions)
            .FirstAsync();
});

app.MapGet("/summary", (
    [FromServices] PokemonDb db) => {
        var generationData = db.PokemonData.GroupBy(d => d.Generation).Select(g => new { Name = g.Key, Count = g.Count() });
        var totalCount = db.PokemonData.Count();
        var typeCount = db.PokemonTypes.Include(t => t.Pokemon).Select(t => new { t.ID, t.Name, Count = t.Pokemon.Count() });

        return new {
            Generation = generationData,
            TypeData = typeCount,
            Total = totalCount,
        };
});

app.Run();
