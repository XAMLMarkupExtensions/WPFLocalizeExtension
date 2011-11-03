namespace XamlLocalizationTest
{
    /// <summary>
    /// Describes the priority type.
    /// </summary>
    public enum PriorityType
    {
        /// <summary>
        /// Undefined priority.
        /// </summary>
        Undifined,

        /// <summary>
        /// High priority.
        /// </summary>
        High,

        /// <summary>
        /// Medium proiority.
        /// </summary>
        Medium,

        /// <summary>
        /// Low priority.
        /// </summary>
        Low
    }

    /// <summary>
    /// Implements the TaskItem.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public PriorityType Priority { get; set; }

        /// <summary>
        /// Gets or sets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string TaskName { get; set; }
    }
}