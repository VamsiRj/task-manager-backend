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

        private readonly AppDbContext assignContext;

        public TaskAssignmentController(AppDbContext context)

        {

            this.assignContext = context;

        }



        //Admin Assigning task to users and cant assign task to another admin

        [HttpPost("{taskId}/assign/{userId}")]

        public IActionResult AssignTask(int taskId, int userId, [FromHeader] int adminId)

        {

            var admin = assignContext.Users.Find(adminId);

            if (admin == null || admin.Role != "Admin")

            {

                return Unauthorized("Only admins can assign tasks");

            }

            var task = assignContext.Tasks.Find(taskId);

            if (task == null)

            {

                return NotFound("Task not found");

            }

            var user = assignContext.Users.Find(userId);

            if (user == null)

            {

                return NotFound("User not found");

            }

            if (user.Role == "Admin")

            {

                return BadRequest("Admins cannot assign tasks to another admin");

            }

            var existingAssignment = assignContext.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId);

            if (existingAssignment != null)

            {

                return BadRequest("This task is already assigned to another user.");

            }

            var assignment = new TaskAssignment { TaskId = taskId, UserId = userId };

            assignContext.TaskAssignments.Add(assignment);

            assignContext.SaveChanges();

            return Ok("Task assigned successfully");

        }

        //User or Team Member can update the task status

        [HttpPut("{taskId}/status")]

        public IActionResult UpdateTaskStatus(int taskId, [FromBody] string newStatus, [FromHeader] int userId)

        {

            var assignment = assignContext.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId && ta.UserId == userId);

            if (assignment == null)

            {

                return Unauthorized("You can only update your assigned tasks");

            }

            var task = assignContext.Tasks.Find(taskId);

            if (task == null)

            {

                return NotFound("Task not found");

            }

            task.Status = newStatus;

            assignContext.SaveChanges();

            return Ok("Task status updated");

        }

        // Admin can Unassign the task to user

        [HttpDelete("{taskId}/unassign/{userId}")]

        public IActionResult UnassignTask(int taskId, int userId, [FromHeader] int adminId)

        {

            var admin = assignContext.Users.Find(adminId);

            if (admin == null || admin.Role != "Admin")

            {

                return Unauthorized("Only admins can unassign tasks");

            }

            var assignment = assignContext.TaskAssignments.FirstOrDefault(ta => ta.TaskId == taskId && ta.UserId == userId);

            if (assignment == null)

            {

                return NotFound("Assignment not found");

            }

            assignContext.TaskAssignments.Remove(assignment);

            assignContext.SaveChanges();

            return Ok("Task unassigned successfully");

        }


        // View All task assigned to users

        [HttpGet("user/{userId}")]

        public IActionResult GetUserTasks(int userId)

        {

            var tasks = assignContext.TaskAssignments

                .Where(ta => ta.UserId == userId)

                .Join(assignContext.Tasks, ta => ta.TaskId, t => t.TaskId, (ta, t) => t)

                .ToList();

            if (!tasks.Any())

            {

                return NotFound("No tasks assigned to this user");

            }

            return Ok(tasks);

        }

        // View all task by taskId,userid,TaskAssignId

        [HttpGet("all")]

        public IActionResult GetAllTaskAssignments()

        {

            var assignments = assignContext.TaskAssignments

                .Select(ta => new

                {

                    ta.TaskAssignId,

                    ta.TaskId,

                    ta.UserId

                })

                .ToList();

            if (!assignments.Any())

            {

                return Ok(new List<object>());

            }

            return Ok(assignments);

        }

    }

}

