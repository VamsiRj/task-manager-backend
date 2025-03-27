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
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Admin can create a new task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Models.Task task, [FromHeader] int adminId)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can create tasks");

            task.OwnerId = adminId; // Assign the admin who created the task

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // ✅ Get all tasks
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var details = await _context.Tasks.ToListAsync();
            return Ok(details);
        }

        // ✅ Get a specific task by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }

        // ✅ Admin can update task priority
        [HttpPut("{taskId}/priority")]
        public async Task<IActionResult> UpdateTaskPriority(int taskId, [FromBody] string newPriority, [FromHeader] int adminId)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can update task priority");

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found");

            task.Priority = newPriority;
            await _context.SaveChangesAsync();
            return Ok("Task priority updated successfully");
        }


        // ✅ Search tasks by TaskName, Priority, Date Range, Status, and Tags
        [HttpGet("search")]
        public async Task<IActionResult> SearchTasks(
            [FromQuery] string? taskName,
            [FromQuery] string? priority,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status,
            [FromQuery] string? tags)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(taskName))
                query = query.Where(t => t.Name.ToLower().Contains(taskName.ToLower()));

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(t => t.Priority.ToLower() == priority.ToLower());

            if (startDate.HasValue && endDate.HasValue)
                query = query.Where(t => t.TargetDate >= startDate && t.TargetDate <= endDate);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrWhiteSpace(tags))
                query = query.Where(t => t.Tags.ToLower().Contains(tags.ToLower()));

            var result = await query.ToListAsync();

            if (!result.Any())
                return NotFound("No tasks found matching the criteria.");

            return Ok(result);
        }

        // ✅ Delete a task (Only Admins)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id, [FromHeader] int adminId)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can delete tasks");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound("Task not found");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok("Task deleted successfully");
        }
    }
}
