
using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace CourseManagement.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;

        private readonly CourseManagementDbContext _context;

        public CourseController(ILogger<CourseController> logger, CourseManagementDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index(int id)
        {
            var teacherId = User.FindFirstValue("TeacherId");

            if (teacherId == null)
            {
                return RedirectToAction("Login", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }

            var courses = _context.Courses
                .Where(c => c.Teacher.teacherId == parsedId)
                .ToList();

            return View(courses);
        }

        public IActionResult List()
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (teacherId == null)
            {
                return RedirectToAction("Login", "Authen");
            }
            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }
            var courses = _context.Courses
                .Where(c => c.Teacher.teacherId == parsedId)
                .ToList();
            return View(courses);
        }

        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == id);
            return View(course);
        }
        public IActionResult Delete(int id)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Edit(Course model)
        {
            var course = _context.Courses.SingleOrDefault(c => c.courseId == model.courseId);
            if (course != null)
            {
                course.courseName = model.courseName;
                course.courseCode = model.courseCode;
                course.courseSubject = model.courseSubject;

                _context.Courses.Update(course);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Add(Course model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                return BadRequest("Không tìm thấy giáo viên.");
            }
            model.Teacher = teacher;
            _context.Courses.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpGet]
        public IActionResult Student(int id)
        {
            var enrollments = _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseId == id)
                .Where(e => e.isActive)
                .ToList();

            var course = _context.Courses.FirstOrDefault(c => c.courseId == id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.course = course;
            return View(enrollments);
        }

        [HttpGet]
        public IActionResult Enroll(int id)
        {
            var teacherId = User.FindFirstValue("TeacherId");

            if (teacherId == null)
            {
                return RedirectToAction("NeedLogin", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                return RedirectToAction("Login", "Authen");
            }

            var students = _context.Students
                .Where(s => s.teacher != null && s.teacher.teacherId == parsedId)
                .ToList();

            var enrollments = _context.Enrollments
                .Where(e => e.CourseId == id)
                .Where(e => e.isActive)
                .ToList();

            var unenrolledStudents = students
                .Where(s => !enrollments.Any(e => e.StudentId == s.studentId))
                .ToList();


            var course = _context.Courses.FirstOrDefault(c => c.courseId == id);
            ViewBag.Course = course;
            return View(unenrolledStudents);
        }

        [HttpPost]
        public IActionResult Enroll(int courseId, int studentId)
        {
            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            var student = _context.Students.FirstOrDefault(s => s.studentId == studentId);
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                return BadRequest("Không tìm thấy giáo viên.");
            }
            Enrollment enrollment = new Enrollment();
            enrollment.CourseId = courseId;
            enrollment.StudentId = studentId;
            enrollment.Course = course;
            enrollment.Student = student;
            enrollment.TeacherId = parsedTeacherId;
            enrollment.Teacher = teacher;
            enrollment.enrollmentDate = DateTime.Now;
            enrollment.isActive = true;

            course.enrollments.Add(enrollment);
            student.enrollments.Add(enrollment);

            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();
            return RedirectToAction("Enroll");

        }

        [HttpGet]
        public IActionResult ViewStudents(int id)
        {
            var course = _context.Courses
                .Include(c => c.enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefault(c => c.courseId == id);

            var enrollments = course?.enrollments
                .Where(e => e.isActive)
                .ToList();

            if (course == null || enrollments == null)
            {
                return NotFound();
            }

            ViewBag.Course = course;
            return View(enrollments);
        }

        [HttpPost]
        public IActionResult UpdateScores(int CourseId, int StudentId, int EnrollmentId, double ProgressScore, double TestScore)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.enrollmentId == EnrollmentId);
            if (enrollment == null)
            {
                return NotFound();
            }

            enrollment.ProgressScore = ProgressScore;
            enrollment.TestScore = TestScore;
            enrollment.setFinalScore();
            enrollment.updatePerformance();

            _context.SaveChanges();

            return RedirectToAction("ViewStudents", new { id = CourseId });
        }

        [HttpPost]
        public IActionResult RemoveStudent(int enrollId, int courseId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.enrollmentId == enrollId);
            enrollment.isActive = false;
            enrollment.unenrollmentDate = DateTime.Now;

            _context.Enrollments.Update(enrollment);
            _context.SaveChanges();
            return RedirectToAction("Student", new { id = courseId });
        }



        [HttpGet]
        public IActionResult Financial(int id)
        {
            var course = _context.Courses
                .Include(c => c.enrollments)
                .ThenInclude(e => e.Student)
                .FirstOrDefault(c => c.courseId == id);
            var revenues = _context.Revenues
                .Where(r => r.CourseId == id)
                .ToList();
            var expenses = _context.Expenses
                .Where(e => e.CourseId == id)
                .ToList();
            if (course == null || revenues == null || expenses == null)
            {
                return NotFound();
            }
            double totalRevenue = 0;
            double totalExpense = 0;
            foreach (var revenue in revenues)
            {
                totalRevenue += revenue.amount;
            }
            foreach (var expense in expenses)
            {
                totalExpense += expense.amount;
            }
            double netIncome = totalRevenue - totalExpense;
            ViewBag.Course = course;
            ViewBag.Revenues = revenues;
            ViewBag.Expenses = expenses;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.NetIncome = netIncome;
            return View();

        }
    }
}
