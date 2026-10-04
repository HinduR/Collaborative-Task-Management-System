namespace Shared.Common.Model
{
	public abstract class BaseModel
	{
		/// <summary>
		/// Creation time
		/// </summary>
		public DateTime CreatedAt  { get; set; }
		/// <summary>
		/// Last updated time
		/// </summary>
		public DateTime UpdatedAt  { get; set; }
		/// <summary>
		/// Created by
		/// </summary>
		public Guid CreatedBy { get; set; }
		/// <summary>
		/// Last updated by
		/// </summary>
		public Guid UpdatedBy { get; set; }
		/// <summary>
		/// Status of the record
		/// </summary>
		public bool IsActive { get; set; }
	}
}