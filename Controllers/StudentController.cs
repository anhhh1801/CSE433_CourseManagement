using System.Linq;
using System.Security.Claims;
using CourseManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly CourseManagementDbContext _context;

        public StudentController(CourseManagementDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
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
                .Include(s => s.enrollments)
                .ThenInclude(e => e.Course)
                .Where(s => s.teacher != null && s.teacher.teacherId == parsedId)
                .ToList();

            return View(students);
        }

        public IActionResult List(string keyword)
        {
            var teacherId = User.FindFirstValue("TeacherId");

            if (teacherId == null)
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return RedirectToAction("NeedLogin", "Authen");
            }

            if (!int.TryParse(teacherId, out int parsedId))
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return RedirectToAction("Login", "Authen");
            }

            // Truy vấn dữ liệu kết hợp lọc nếu có từ khóa
            var studentsQuery = _context.Students
                .Include(s => s.enrollments)
                    .ThenInclude(e => e.Course)
                .Where(s => s.teacher != null && s.teacher.teacherId == parsedId);

            if (!string.IsNullOrEmpty(keyword))
            {
                studentsQuery = studentsQuery.Where(s => s.studentName.Contains(keyword));
            }

            var students = studentsQuery.ToList();

            ViewBag.Keyword = keyword; // Giữ lại keyword để hiển thị lại trong view

            return View(students);
        }


        public IActionResult SearchList(string keyword)
        {
            ViewBag.Keyword = keyword;

            // Giả sử bạn có danh sách sinh viên từ database
            var students = _context.Students.AsQueryable();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrEmpty(keyword))
            {
                students = students.Where(s => s.studentName.Contains(keyword));
            }

            return RedirectToAction("List");
        }
        


        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Student model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return BadRequest("Không tìm thấy giáo viên.");
            }
            model.teacher = teacher;
            model.Avatar = "/img/avatar_default.jpg";
            teacher.students.Add(model);
            _context.Users.Update(teacher);
            _context.Students.Add(model);
            _context.SaveChanges();
            ViewBag.Teacher = teacher;
            TempData["Message"] = "Add Student Successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(Student model)
        {
            var teacherId = User.FindFirstValue("TeacherId");
            if (string.IsNullOrEmpty(teacherId) || !int.TryParse(teacherId, out int parsedTeacherId))
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return RedirectToAction("Login", "Authen"); // Redirect if teacher is not logged in
            }
            var teacher = _context.Users.FirstOrDefault(t => t.teacherId == parsedTeacherId);
            if (teacher == null)
            {
                TempData["Error"] = "User Not Found! Please Login Again!";
                return BadRequest("Không tìm thấy giáo viên.");
            }
            model.teacher = teacher;
            model.Avatar = "/img/avatar_default.jpg";
            teacher.students.Add(model);
            _context.Users.Update(teacher);
            _context.Students.Add(model);
            _context.SaveChanges();
            ViewBag.Teacher = teacher;
            TempData["Message"] = "Add Student Successfully!";
            return RedirectToAction("List");
        }

        public IActionResult Delete(int id)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == id);
            var enrollments = _context.Enrollments
                .Where(e => e.StudentId == id)
                .ToList();
            if (student != null)
            {
                foreach(var enroll in enrollments)
                {
                    _context.Enrollments.Remove(enroll);
                    _context.SaveChanges();
                }
                _context.Students.Remove(student);
                _context.SaveChanges();
                TempData["Message"] = "Delete Student Successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == id);
            if (student == null)
            {
                TempData["Error"] = "Student Not Found!";
                return View();
            }
            return View(student);
        }

        [HttpPost]
        public IActionResult Edit(Student model)
        {
            var student = _context.Students.SingleOrDefault(s => s.studentId == model.studentId);
            if (student != null)
            {
                student.studentName = model.studentName;
                student.EntryDate = model.EntryDate;
                student.parentNumber = model.parentNumber;
                _context.Students.Update(student);
                _context.SaveChanges();
                TempData["Message"] = "Update Student Information Successfully!";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(int id, IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction("Index");
            }

            try
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.studentId == id);
                if (student == null)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                // Đường dẫn lưu file avatar
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                var fileName = Path.GetFileName(avatar.FileName);
                var filePath = Path.Combine(uploadDir, fileName);

                // Ghi file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Cập nhật tên file avatar cho học sinh
                student.Avatar = "/img/" + avatar.FileName;
                _context.Students.Update(student);
                _context.SaveChanges();

                TempData["Message"] = $"Avatar updated successfully for {student.studentName}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not upload avatar: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Courses(int id)
        {
            var course =_context.Courses
                .Where(c => c.enrollments.Any(e => e.StudentId == id))
                .ToList();
            return View(course);

        }
    }
}

