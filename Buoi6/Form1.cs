using Buoi6.Model;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Data.Entity;


namespace Buoi6
{
    public partial class Form1 : Form
    {
        private StudentContextDB context;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            context = new StudentContextDB();
            LoadFaculties();
            LoadStudents();
            dgvStudents.CellValueChanged += dgvStudents_CellValueChanged;
            dgvStudents.CellClick += dgvStudents_CellClick;
        }

        private void LoadFaculties()
        {
            var list = context.Faculties.ToList();
            cmbKhoa.DataSource = list;
            cmbKhoa.DisplayMember = "FacultyName";
            cmbKhoa.ValueMember = "FacultyID";
        }

        private void LoadStudents()
        {
            var data = context.Students
                .Include(s => s.Faculty)
                .Select(s => new
                {
                    s.StudentID,
                    s.FullName,
                    FacultyName = s.Faculty.FacultyName,
                    s.AverageScore,
                    s.FacultyID
                })
                .ToList();

            dgvStudents.DataSource = data;

            dgvStudents.Columns["StudentID"].HeaderText = "Mã SV";
            dgvStudents.Columns["FullName"].HeaderText = "Họ tên";
            dgvStudents.Columns["FacultyName"].HeaderText = "Khoa";
            dgvStudents.Columns["AverageScore"].HeaderText = "Điểm TB";

            if (dgvStudents.Columns["FacultyID"] != null)
                dgvStudents.Columns["FacultyID"].Visible = false;

            dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void ResetForm()
        {
            txtMSSV.Clear();
            txtHoTen.Clear();
            txtDiemTB.Clear();
            cmbKhoa.SelectedIndex = -1;
            txtMSSV.Enabled = true;
            txtMSSV.Focus();
        }

        private bool ValidateInput()
        {
            string id = txtMSSV.Text.Trim();
            if (string.IsNullOrEmpty(id) || id.Length != 10)
            {
                MessageBox.Show("Mã sinh viên phải có đúng 10 ký tự.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Nhập họ tên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!float.TryParse(txtDiemTB.Text.Trim(), out float diem) || diem < 0 || diem > 10)
            {
                MessageBox.Show("Điểm TB phải là số trong khoảng 0 đến 10.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbKhoa.SelectedItem == null)
            {
                MessageBox.Show("Chọn khoa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string id = txtMSSV.Text.Trim();
            if (context.Students.Any(s => s.StudentID == id))
            {
                MessageBox.Show("MSSV đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float diem = float.Parse(txtDiemTB.Text.Trim());

            var student = new Student
            {
                StudentID = id,
                FullName = txtHoTen.Text.Trim(),
                AverageScore = diem,
                FacultyID = (int)cmbKhoa.SelectedValue
            };

            context.Students.Add(student);
            context.SaveChanges();

            MessageBox.Show("Thêm sinh viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStudents();
            ResetForm();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string id = txtMSSV.Text.Trim();
            var student = context.Students.Find(id);
            if (student == null)
            {
                MessageBox.Show("Không tìm thấy MSSV.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            student.FullName = txtHoTen.Text.Trim();
            student.AverageScore = float.Parse(txtDiemTB.Text.Trim());
            student.FacultyID = (int)cmbKhoa.SelectedValue;

            context.SaveChanges();
            MessageBox.Show("Cập nhật dữ liệu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadStudents();
        }

        private void btXoa_Click(object sender, EventArgs e)
        {
            if (dgvStudents.CurrentRow == null)
            {
                MessageBox.Show("Hãy chọn 1 sinh viên trong danh sách để xóa!", "Thông báo");
                return;
            }

            string id = dgvStudents.CurrentRow.Cells["StudentID"].Value.ToString();
            var student = context.Students.Find(id);

            if (student == null)
            {
                MessageBox.Show("Không tìm thấy MSSV cần xóa!", "Lỗi");
                return;
            }

            if (MessageBox.Show($"Bạn có chắc muốn xóa sinh viên {student.FullName} không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                context.Students.Remove(student);
                context.SaveChanges();
                MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudents();
                ResetForm();
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát chương trình không?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudents.Rows[e.RowIndex];
                txtMSSV.Text = row.Cells["StudentID"].Value.ToString();
                txtHoTen.Text = row.Cells["FullName"].Value.ToString();
                txtDiemTB.Text = row.Cells["AverageScore"].Value.ToString();
                cmbKhoa.SelectedValue = row.Cells["FacultyID"].Value;
                txtMSSV.Enabled = false;
            }
        }

        private void dgvStudents_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && dgvStudents.Columns[e.ColumnIndex].Name != "FacultyName")
                {
                    string id = dgvStudents.Rows[e.RowIndex].Cells["StudentID"].Value.ToString();
                    var student = context.Students.Find(id);

                    if (student != null)
                    {
                        student.FullName = dgvStudents.Rows[e.RowIndex].Cells["FullName"].Value.ToString();

                        if (float.TryParse(dgvStudents.Rows[e.RowIndex].Cells["AverageScore"].Value?.ToString(), out float diem))
                            student.AverageScore = diem;

                        if (dgvStudents.Columns.Contains("FacultyID"))
                        {
                            int facultyID = Convert.ToInt32(dgvStudents.Rows[e.RowIndex].Cells["FacultyID"].Value);
                            student.FacultyID = facultyID;
                        }

                        context.SaveChanges();
                        Console.WriteLine($"Đã lưu thay đổi MSSV {id}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
