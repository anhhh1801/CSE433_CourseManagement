
using CourseManagement.Models;
using CourseManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace CourseManagement.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;

        private readonly CourseManagementDbContext _context;
        private readonly FinancialService _financialService;

        public CourseController(ILogger<CourseController> logger, CourseManagementDbContext context, FinancialService financialService)
        {
            _logger = logger;
            _context = context;
            _financialService = financialService;
        }
        public IActionResult Index(int id)
        {
            var teacherId = User.FindFirstValue("TeacherId");

            if (teacherId == null)
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen");
            }

            var courses = _context.Courses
                .Include(c => c.enrollments)
                .Where(c => c.Teacher.teacherId == parsedId)
                .ToList();

            return View(courses);
        }

        public IActionResult List()
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (teacherId == null)
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen");
            }
            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen");
            }
            var courses = _context.Courses
                .Include(c => c.enrollments)
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
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == course.TeacherId);
            if (course == null)
            {
                TempData["Error"] = "Course not found!";
                return RedirectToAction("Financial");
            }

            if (teacher == null)
            {
                TempData["Error"] = "Teacher not found!";
                return RedirectToAction("Financial");
            }


            double courseNetIncome = course.netIncome ?? 0;
            
            var incomes = _context.Incomes
                .Where(i => i.CourseId == course.courseId && i.Description == "Enroll")
                .ToList();
            // Xoá tất cả các Income liên quan đến khoá học này
            foreach (var income in incomes)
            {
                _context.Incomes.Remove(income);
                _context.SaveChanges();
            }
            var expenses = _context.Expenses
                .Where(e => e.CourseId == course.courseId)
                .ToList();
            // Xoá tất cả các Expense liên quan đến khoá học này
            foreach (var expense in expenses)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }



            teacher.NetIncome -= courseNetIncome; // Trừ netIncome của giáo viên
            _context.Users.Update(teacher); // Cập nhật giáo viên\



            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
                TempData["Message"] = "Delete Sucessfully!";
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
                course.TuitionFee = model.TuitionFee;

                _context.Courses.Update(course);
                _context.SaveChanges();
                TempData["Message"] = "Update Sucessfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Add(Course model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return BadRequest("Không tìm thấy giáo viên.");
            }
            teacher.courses.Add(model);
            model.Teacher = teacher;
            _context.Users.Update(teacher);
            _context.Courses.Add(model);
            _context.SaveChanges();
            TempData["Message"] = "Add Course Sucessfully!";
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
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("NeedLogin", "Authen");
            }

            int parsedId;
            if (!int.TryParse(teacherId, out parsedId))
            {
                TempData["Error"] = "User Not Found! Please Login again!";
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
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                TempData["Error"] = "User Not Found! Please Login again!";
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }

            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            var student = _context.Students.FirstOrDefault(s => s.studentId == studentId);
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null || student == null || course == null)
            {
                TempData["Error"] = "User Not Found! Cannot find data!";
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
            teacher.enrollments.Add(enrollment);

            _context.Users.Update(teacher);
            _context.Enrollments.Add(enrollment);


            var income = new Incomes
            {
                CourseId = courseId,
                Amount = course.TuitionFee,
                Description = "Enroll for " + student.studentName
            };
            _context.Incomes.Add(income);
            _context.SaveChanges();

            //  Gọi service để cập nhật netIncome
            _financialService.UpdateCourseNetIncome(courseId);
            _financialService.UpdateTeacherNetIncome(parsedTeacherId);




            TempData["Message"] = "Enroll Student Into Course Successfully!";
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
            TempData["Message"] = "Update Score For Student Successfully!";

            return RedirectToAction("ViewStudents", new { id = CourseId });
        }

        [HttpPost]
        public IActionResult RemoveStudent(int enrollId, int courseId)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.enrollmentId == enrollId);
            if (enrollment == null)
            {
                TempData["Error"] = "Enrollment not found!";
                return RedirectToAction("Student", new { id = courseId });
            }

            var course = _context.Courses.FirstOrDefault(c => c.courseId == courseId);
            if (course == null)
            {
                TempData["Error"] = "Course not found!";
                return RedirectToAction("Student", new { id = courseId });
            }

            //  Xoá Income đúng với học phí và mô tả Enroll
            var income = _context.Incomes
                .Where(i =>
                    i.CourseId == courseId &&
                    i.Description == "Enroll" &&
                    Math.Abs(i.Amount - course.TuitionFee) < 0.001)
                .OrderByDescending(i => i.IncomeId)
                .FirstOrDefault();

            if (income != null)
            {
                _context.Incomes.Remove(income);
            }

            _context.Enrollments.Remove(enrollment);
            _context.SaveChanges();

            //  Cập nhật lại NetIncome
            _financialService.UpdateCourseNetIncome(courseId);
            _financialService.UpdateTeacherNetIncome(course.TeacherId);

            TempData["Message"] = "Unenroll Student Successfully!";
            return RedirectToAction("Student", new { id = courseId });
        }


        //[HttpGet]
        //public IActionResult Financial(int id)
        //{


        //    var course = _context.Courses
        //        .Include(c => c.enrollments)
        //        .ThenInclude(e => e.Student)
        //        .FirstOrDefault(c => c.courseId == id);
        //    var incomes = _context.Incomes
        //        .Where(r => r.CourseId == id)
        //        .ToList();
        //    var expenses = _context.Expenses
        //        .Where(e => e.CourseId == id)
        //        .ToList();
        //    if (course == null || incomes == null || expenses == null)
        //    {
        //        return NotFound();
        //    }
        //    double totalIncomes = 0;
        //    double totalExpenses = 0;
        //    foreach (var revenue in incomes)
        //    {
        //        if (revenue == null)
        //        {
        //            revenue.Amount = 0; 
        //        }
        //        totalIncomes += revenue.Amount;
        //    }
        //    foreach (var expense in expenses)
        //    {
        //        if(expense == null)
        //        {
        //            expense.Amount = 0;
        //        }
        //        totalExpenses += expense.Amount;
        //    }
        //    double netIncome = totalIncomes - totalExpenses;

        //    ViewBag.Course = course;
        //    ViewBag.Incomes = incomes;
        //    ViewBag.Expenses = expenses;
        //    ViewBag.TotalIncomes = totalIncomes;
        //    ViewBag.TotalExpenses = totalExpenses;
        //    ViewBag.NetIncome = netIncome;
        //    return View();

        //}
        [HttpGet]
        public IActionResult Financial(int id)
        {
            var course = _context.Courses
                .Include(c => c.enrollments)
                .ThenInclude(e => e.Student)
                .Include(c => c.Teacher)
                .FirstOrDefault(c => c.courseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var incomes = _context.Incomes.Where(r => r.CourseId == id).ToList();
            var expenses = _context.Expenses.Where(e => e.CourseId == id).ToList();

            double totalIncomes = _financialService.GetTotalIncome(id);
            double totalExpenses = _financialService.GetTotalExpense(id);
            double netIncome = totalIncomes - totalExpenses;

            ViewBag.Course = course;
            ViewBag.Incomes = incomes;
            ViewBag.Expenses = expenses;
            ViewBag.TotalIncomes = totalIncomes;
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.NetIncome = netIncome;

            return View();
        }

    }
}
