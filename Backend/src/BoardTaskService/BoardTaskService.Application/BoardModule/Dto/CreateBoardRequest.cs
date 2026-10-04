namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>Represents a request to create a board.</summary>
public class CreateBoardRequest
{
    /// <summary>Gets or sets the name of the board to create.</summary>
    public string Name { get; set; } = string.Empty;
}
