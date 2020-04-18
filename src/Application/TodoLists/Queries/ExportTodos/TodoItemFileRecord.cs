using PlexRipper.Application.Common.Mappings;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.TodoLists.Queries.ExportTodos
{
    public class TodoItemRecord : IMapFrom<TodoItem>
    {
        public string Title { get; set; }

        public bool Done { get; set; }
    }
}
