using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AcademicManagement;

namespace Lab2.Pages
{
    public class RegistrationModel : PageModel
    {
        public List<Student> Students { get; set; } = new List<Student>();

        public List<Course> Courses { get; set; } = new List<Course>();

        public List<AcademicRecord> AcademicRecords { get; set; } = new List<AcademicRecord>();

        // SelectList for the Razor dropdown
        public List<SelectListItem> StudentSelectList { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public string? SelectedStudentId { get; set; }

        [BindProperty]
        public List<string> SelectedCourseCodes { get; set; } = new List<string>();

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            Students = DataAccess.GetAllStudents();
        }

        public void OnPostStudentSelected()
        {
            Students = DataAccess.GetAllStudents();

            if (SelectedStudentId == "0" || string.IsNullOrEmpty(SelectedStudentId))
            {
                ErrorMessage = "Please select a student.";
                return;
            }

            AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);

            // Load courses so we can show course titles for any registered records
            Courses = DataAccess.GetAllCourses();

            ErrorMessage = string.Empty;
        }

        public void OnPostRegister()
        {
            // Ensure students and courses are loaded so the page can redisplay if validation fails
            Students = DataAccess.GetAllStudents();
            Courses = DataAccess.GetAllCourses();

            if (SelectedCourseCodes == null || SelectedCourseCodes.Count == 0)
            {
                ErrorMessage = "You must select at least one course.";
                return;
            }

            foreach (string courseCode in SelectedCourseCodes)
            {
                var record = new AcademicRecord
                {
                    StudentId = SelectedStudentId,
                    CourseCode = courseCode
                };

                DataAccess.AddAcademicRecord(record);
            }

            // Refresh registered records and course metadata
            AcademicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);
            Courses = DataAccess.GetAllCourses();

            ErrorMessage = string.Empty;
        }

        private void LoadStudents()
        {
            Students = DataAccess.GetAllStudents() ?? new List<Student>();
            StudentSelectList = new List<SelectListItem> { new SelectListItem("Choose a student...", "0") };

            foreach (var s in Students)
            {
                var id = GetStudentId(s);
                var name = GetStudentName(s);
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                {
                    StudentSelectList.Add(new SelectListItem(name, id));
                }
            }
        }

        private static string GetStudentId(Student s)
        {
            var t = s.GetType();
            var p = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                 ?? t.GetProperty("StudentId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return p?.GetValue(s)?.ToString() ?? string.Empty;
        }

        private static string GetStudentName(Student s)
        {
            var t = s.GetType();
            var p = t.GetProperty("FullName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                 ?? t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                 ?? t.GetProperty("StudentName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var value = p?.GetValue(s)?.ToString();
            return value ?? s.ToString() ?? string.Empty;
        }
    }
}
