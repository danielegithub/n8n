
// Definizione della classe statica con i metodi per mappare gli endpoint.
public static class TodosApi
{
    public static void MapTodosApi(this IEndpointRouteBuilder app, List<TodoItem> todos)
    {

        app.MapGet("/todos", () =>
        {
            return Results.Ok(todos);
        });
        
        app.MapGet("/todos/{id}", (string id) =>
        {
            var todo = todos.FirstOrDefault(t => t.Id == id);

            if (todo is not null)
            {
                return Results.Ok(todo);
            }
            return Results.NotFound();
        });

        app.MapPost("/todos", (TodoItem newTodo) =>
        {

            todos.Add(newTodo);
            return Results.Created($"/todos/{newTodo.Id}", newTodo);
        });

        app.MapPut("/todos/{id}", (string id, TodoItem updatedTodo) =>
        {
            var todo = todos.FirstOrDefault(t => t.Id == id);

            if (todo is null)
            {
                return Results.NotFound();
            }
            todo = updatedTodo;
            return Results.NoContent();
        });

        app.MapDelete("/todos/{id}", (string id) =>
        {
            var removed = todos.RemoveAll(t => t.Id == id);

            if (removed > 0)
            {
                return Results.NoContent();
            }
            return Results.NotFound();
        });

    }
}