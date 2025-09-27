using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using TaskManagementAPI.Data;

using TaskManagementAPI.Models;

using System.Linq;

using System.Threading.Tasks;

using System;

namespace TaskManagementAPI.Controllers

{

    [Route("api/tasks")]

    [ApiController]

    public class TaskController : ControllerBase

    {

        private readonly AppDbContext taskContext;

        public TaskController(AppDbContext context)

        {

            taskContext = context;

        }

        // Admin will create new task

        [HttpPost]

        public async Task<IActionResult> CreateTask([FromBody] Models.Task task, [FromHeader] int adminId)

        {

            var admin = await taskContext.Users.FindAsync(adminId);

            if (admin == null || admin.Role != "Admin")

            {

                return Unauthorized("Only admins can create tasks");

            }

            task.OwnerId = adminId;//admin id  who assigned the task

            await taskContext.Tasks.AddAsync(task);

            await taskContext.SaveChangesAsync();

            return Ok(task);

        }

        // to view all tasks

        [HttpGet]

        public async Task<IActionResult> GetTasks()

        {

            var details = await taskContext.Tasks.ToListAsync();

            return Ok(details);

        }

        //View task by Id

        [HttpGet("{id}")]

        public async Task<IActionResult> GetTaskById(int id)

        {

            var task = await taskContext.Tasks.FindAsync(id);

            if (task == null)

            {

                return NotFound("Task not found");

            }

            return Ok(task);

        }

        //  admin can update the task priority

        [HttpPut("{taskId}/priority")]

        public async Task<IActionResult> UpdateTaskPriority(int taskId, [FromBody] string newPriority, [FromHeader] int adminId)

        {

            var admin = await taskContext.Users.FindAsync(adminId);

            if (admin == null || admin.Role != "Admin")

            {

                return Unauthorized("Only admins can update task priority");

            }

            var task = await taskContext.Tasks.FindAsync(taskId);

            if (task == null)

            {

                return NotFound("Task not found");

            }

            task.Priority = newPriority;

            await taskContext.SaveChangesAsync();

            return Ok("Task priority updated successfully");

        }


        // find task by tag,name,priority,status etc

        [HttpGet("search")]

        public async Task<IActionResult> SearchTasks(

            [FromQuery] string? taskName,

            [FromQuery] string? priority,

            [FromQuery] DateTime? startDate,

            [FromQuery] DateTime? endDate,

            [FromQuery] string? status,

            [FromQuery] string? tags)

        {

            var query = taskContext.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(taskName))

            {

                query = query.Where(t => t.Name.ToLower().Contains(taskName.ToLower()));

            }

            if (!string.IsNullOrWhiteSpace(priority))

            {

                query = query.Where(t => t.Priority.ToLower() == priority.ToLower());

            }

            if (startDate.HasValue && endDate.HasValue)

            {

                query = query.Where(t => t.TargetDate >= startDate && t.TargetDate <= endDate);

            }

            if (!string.IsNullOrWhiteSpace(status))

            {

                query = query.Where(t => t.Status.ToLower() == status.ToLower());

            }

            if (!string.IsNullOrWhiteSpace(tags))

            {

                query = query.Where(t => t.Tags.ToLower().Contains(tags.ToLower()));

            }

            var result = await query.ToListAsync();

            if (!result.Any())

                return NotFound("No tasks found matching the criteria.");

            return Ok(result);

        }

        //Admin can delete the task

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteTask(int id, [FromHeader] int adminId)

        {

            var admin = await taskContext.Users.FindAsync(adminId);

            if (admin == null || admin.Role != "Admin")

                return Unauthorized("Only admins can delete tasks");

            var task = await taskContext.Tasks.FindAsync(id);

            if (task == null)

            {

                return NotFound("Task not found");

            }

            taskContext.Tasks.Remove(task);

            await taskContext.SaveChangesAsync();

            return Ok("Task deleted successfully");

        }

    }

}

