using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;
using System.Linq;
using Microsoft.VisualBasic;

namespace TaskManagementAPI.Controllers
{
    [Route("api/task-assignments")]
    [ApiController]
    public class TaskAssignmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskAssignmentController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Admin assigns a task to a user
        // ✅ Admin assigns a task to a user (excluding another admin)
        [HttpPost("{taskId}/assign/{userId}")]
        public IActionResult AssignTask(int taskId, int userId, [FromHeader] int adminId)
        {
            var admin = _context.Users.Find(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can assign tasks");

            var task = _context.Tasks.Find(taskId);
            if (task == null) return NotFound("Task not found");

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound("User not found");

            if (user.Role == "Admin")
                return BadRequest("Admins cannot assign tasks to another admin");

            var existingAssignment = _context.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId);
            if (existingAssignment != null)
                return BadRequest("This task is already assigned to another user.");

            var assignment = new TaskAssignment { TaskId = taskId, UserId = userId };
            _context.TaskAssignments.Add(assignment);
            _context.SaveChanges();
            return Ok("Task assigned successfully");
        }

        // ✅ Team member updates their task status
        [HttpPut("{taskId}/status")]
        public IActionResult UpdateTaskStatus(int taskId, [FromBody] string newStatus, [FromHeader] int userId)
        {
            var assignment = _context.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId && ta.UserId == userId);
            if (assignment == null) return Unauthorized("You can only update your assigned tasks");

            var task = _context.Tasks.Find(taskId);
            if (task == null) return NotFound("Task not found");

            task.Status = newStatus;
            _context.SaveChanges();
            return Ok("Task status updated");
        }

        // ✅ Admin can unassign a task from a user
        [HttpDelete("{taskId}/unassign/{userId}")]
        public IActionResult UnassignTask(int taskId, int userId, [FromHeader] int adminId)
        {
            var admin = _context.Users.Find(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can unassign tasks");

            var assignment = _context.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId && ta.UserId == userId);
            if (assignment == null) return NotFound("Assignment not found");

            _context.TaskAssignments.Remove(assignment);
            _context.SaveChanges();
            return Ok("Task unassigned successfully");
        }

        // ✅ Admin can remove all tasks assigned to a user
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> RemoveUserTasks(int userId, [FromHeader] int adminId)
        {
            var admin = await _context.Users.FindAsync(adminId);
            if (admin == null || admin.Role != "Admin")
                return Unauthorized("Only admins can remove tasks assigned to a user");

            var assignments = _context.TaskAssignments.Where(ta => ta.UserId == userId);
            if (!assignments.Any())
                return NotFound("No tasks assigned to this user.");

            _context.TaskAssignments.RemoveRange(assignments);
            await _context.SaveChangesAsync();
            return Ok("All tasks assigned to the user have been removed");
        }

        // ✅ Get all tasks assigned to a user
        [HttpGet("user/{userId}")]
        public IActionResult GetUserTasks(int userId)
        {
            var tasks = _context.TaskAssignments
                .Where(ta => ta.UserId == userId)
                .Join(_context.Tasks, ta => ta.TaskId, t => t.TaskId, (ta, t) => t)
                .ToList();

            if (!tasks.Any()) return NotFound("No tasks assigned to this user");
            return Ok(tasks);
        }

        // ✅ Get all task assignments with TaskAssignId, TaskId, and UserId
        [HttpGet("all")]
        public IActionResult GetAllTaskAssignments()
        {
            var assignments = _context.TaskAssignments
                .Select(ta => new
                {
                    ta.TaskAssignId,
                    ta.TaskId,
                    ta.UserId
                })
                .ToList();

            if (!assignments.Any()) return Ok(new List<object>());
            return Ok(assignments);
        }

    }
}
