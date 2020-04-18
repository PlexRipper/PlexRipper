using PlexRipper.Application.TodoLists.Queries.ExportTodos;
using System.Collections.Generic;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface ICsvFileBuilder
    {
        byte[] BuildTodoItemsFile(IEnumerable<TodoItemRecord> records);
    }
}
