using ClosedXML.Excel;

namespace HotelManagement.UI;

public enum EditFieldType
{
    Text,
    Integer,
    Decimal,
    Choice,
    Date
}

public sealed record EditField(
    string Name,
    string Caption,
    EditFieldType Type = EditFieldType.Text,
    int MaxLength = 100,
    bool Required = true,
    string[]? Choices = null,
    bool ReadOnly = false);

public sealed record GridField(string PropertyName, string Caption, float FillWeight = 15, string? Format = null);

public abstract class EntityGridForm<TRow> : Form where TRow : class
{
    private readonly string _sheetName;
    private readonly IReadOnlyList<GridField> _fields;
    private readonly Func<string, List<TRow>> _load;
    private readonly Func<TRow, string> _key;
    private readonly Func<string?, bool> _edit;
    private readonly Action<string> _delete;
    private readonly Image _rowIcon;
    private readonly Image _editIcon;
    private readonly Image _deleteIcon;
    private readonly string _addCaption;
    private readonly string _editCaption;
    private readonly string _deleteCaption;
    private readonly TextBox _search = new() { MaxLength = 100, Dock = DockStyle.Fill };
    private readonly DataGridView _grid = new();

    protected EntityGridForm(
        string title,
        string sheetName,
        IReadOnlyList<GridField> fields,
        Func<string, List<TRow>> load,
        Func<TRow, string> key,
        Func<string?, bool> edit,
        Action<string> delete,
        Image rowIcon,
        Image editIcon,
        Image deleteIcon,
        string addCaption = "Thêm mới",
        string editCaption = "Cập nhật",
        string deleteCaption = "Xóa")
    {
        Text = title;
        _sheetName = sheetName;
        _fields = fields;
        _load = load;
        _key = key;
        _edit = edit;
        _delete = delete;
        _rowIcon = rowIcon;
        _editIcon = editIcon;
        _deleteIcon = deleteIcon;
        _addCaption = addCaption;
        _editCaption = editCaption;
        _deleteCaption = deleteCaption;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        BackColor = SystemColors.Control;
        BuildLayout();
        Load += (_, _) => ReloadRows();
        _search.TextChanged += (_, _) => ReloadRows();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(8, 7, 8, 7),
            Margin = Padding.Empty
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 167));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 167));

        var searchIcon = new PictureBox
        {
            Image = ResourceImages.FromBytes(Properties.Resources.search),
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(3, 0, 5, 0),
            TabStop = false
        };
        _search.Font = new Font("Microsoft Sans Serif", 13F);
        _search.Margin = new Padding(2, 2, 8, 3);
        toolbar.Controls.Add(searchIcon, 0, 0);
        toolbar.Controls.Add(_search, 1, 0);

        var export = ToolbarButton("Xuất file Excel", Color.DarkTurquoise);
        export.Click += (_, _) => ExportToExcel();
        toolbar.Controls.Add(export, 3, 0);
        var add = ToolbarButton(_addCaption, Color.LightPink);
        add.Click += (_, _) => OpenEditor(null);
        toolbar.Controls.Add(add, 4, 0);
        root.Controls.Add(toolbar, 0, 0);

        ConfigureGrid();
        root.Controls.Add(_grid, 0, 1);
    }

    private static Button ToolbarButton(string text, Color color) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        BackColor = color,
        Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
        Margin = new Padding(3, 0, 3, 0),
        UseVisualStyleBackColor = false
    };

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.BackgroundColor = Color.DarkGray;
        _grid.BorderStyle = BorderStyle.Fixed3D;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.Single;
        _grid.GridColor = SystemColors.ControlDark;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.RowHeadersVisible = false;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.MultiSelect = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 9F);
        _grid.RowTemplate.Height = 40;
        _grid.AutoGenerateColumns = false;
        _grid.Columns.Add(ImageColumn("colIcon", "", _rowIcon, 50));

        foreach (var field in _fields)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = field.PropertyName,
                DataPropertyName = field.PropertyName,
                HeaderText = field.Caption,
                FillWeight = field.FillWeight,
                ReadOnly = true,
                DefaultCellStyle = { Format = field.Format ?? string.Empty }
            });
        }
        _grid.Columns.Add(ImageColumn("colSua", _editCaption, _editIcon, 50));
        _grid.Columns.Add(ImageColumn("colXoa", _deleteCaption, _deleteIcon, 50));
        _grid.CellClick += Grid_CellClick;
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0)
                return;
            var name = _grid.Columns[e.ColumnIndex].Name;
            if (name == "colIcon") e.Value = _rowIcon;
            else if (name == "colSua") e.Value = _editIcon;
            else if (name == "colXoa") e.Value = _deleteIcon;
            else return;
            e.FormattingApplied = true;
        };
    }

    private static DataGridViewImageColumn ImageColumn(string name, string caption, Image image, int width) => new()
    {
        Name = name,
        HeaderText = caption,
        Image = image,
        ImageLayout = DataGridViewImageCellLayout.Zoom,
        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
        Width = width,
        MinimumWidth = width,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private void ReloadRows()
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            _grid.DataSource = _load(_search.Text.Trim());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không tải được danh sách.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not TRow row)
            return;

        var key = _key(row);
        switch (_grid.Columns[e.ColumnIndex].Name)
        {
            case "colSua":
                OpenEditor(key);
                break;
            case "colXoa":
                if (MessageBox.Show($"Bạn có chắc muốn xóa {key}?", "Xác nhận xóa",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        _delete(key);
                        ReloadRows();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không xóa được {key}.\n{ex.Message}", "Lỗi CSDL",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                break;
        }
    }

    private void OpenEditor(string? key)
    {
        try
        {
            if (_edit(key))
                ReloadRows();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không mở được thông tin.\n{ex.Message}", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToExcel()
    {
        if (_grid.Rows.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = $"Xuất {_sheetName}",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"{_sheetName.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        };
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
            return;

        try
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(_sheetName.Length > 31 ? _sheetName[..31] : _sheetName);
            var visibleColumns = _fields
                .Select(field => _grid.Columns[field.PropertyName] ?? throw new InvalidOperationException($"Không tìm thấy cột {field.PropertyName}."))
                .ToList();
            for (int i = 0; i < visibleColumns.Count; i++)
            {
                sheet.Cell(1, i + 1).Value = visibleColumns[i].HeaderText;
                sheet.Cell(1, i + 1).Style.Font.Bold = true;
            }

            var excelRow = 2;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (!row.Visible || row.IsNewRow)
                    continue;
                for (int i = 0; i < visibleColumns.Count; i++)
                    sheet.Cell(excelRow, i + 1).Value = row.Cells[visibleColumns[i].Index].FormattedValue?.ToString() ?? string.Empty;
                excelRow++;
            }
            sheet.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);
            MessageBox.Show("Xuất Excel thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không xuất được Excel.\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

public sealed class EntityEditDialog : Form
{
    private readonly IReadOnlyList<EditField> _fields;
    private readonly IReadOnlyDictionary<string, string> _initialValues;
    private readonly Func<IReadOnlyDictionary<string, string>, bool> _save;
    private readonly string _heading;
    private readonly string _saveCaption;
    private readonly Size _clientSize;
    private readonly bool _twoColumns;
    private readonly Dictionary<string, Control> _inputs = new(StringComparer.Ordinal);

    public EntityEditDialog(
        string title,
        string heading,
        IReadOnlyList<EditField> fields,
        IReadOnlyDictionary<string, string> initialValues,
        Func<IReadOnlyDictionary<string, string>, bool> save,
        Size clientSize,
        bool twoColumns = false,
        string saveCaption = "Thêm mới")
    {
        Text = title;
        _heading = heading;
        _fields = fields;
        _initialValues = initialValues;
        _save = save;
        _clientSize = clientSize;
        _twoColumns = twoColumns;
        _saveCaption = saveCaption;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        Font = new Font("Microsoft Sans Serif", 10F);
        BackColor = SystemColors.Control;
        BuildLayout();
    }

    private void BuildLayout()
    {
        ClientSize = _clientSize;
        var heading = new Label
        {
            Text = _heading,
            Location = new Point(12, 22),
            Size = new Size(ClientSize.Width - 24, 34),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft Sans Serif", 15F, FontStyle.Bold),
            ForeColor = Color.Red
        };
        Controls.Add(heading);

        var body = new Panel
        {
            Location = new Point(24, 76),
            Size = new Size(ClientSize.Width - 48, ClientSize.Height - 156),
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(body);

        var columnCount = _twoColumns ? 2 : 1;
        var rowsPerColumn = (int)Math.Ceiling(_fields.Count / (double)columnCount);
        var rowHeight = Math.Max(42, (body.ClientSize.Height - 12) / Math.Max(1, rowsPerColumn));
        for (var index = 0; index < _fields.Count; index++)
        {
            var field = _fields[index];
            var column = _twoColumns && index >= rowsPerColumn ? 1 : 0;
            var row = _twoColumns ? index % rowsPerColumn : index;
            var columnWidth = body.ClientSize.Width / columnCount;
            var labelWidth = _twoColumns ? Math.Min(140, columnWidth / 3) : Math.Min(200, (int)(columnWidth * 0.36));
            var xOffset = column * columnWidth;
            var y = 5 + row * rowHeight;
            var label = new Label
            {
                Text = field.Caption,
                Location = new Point(xOffset + 8, y + (rowHeight - 28) / 2),
                Size = new Size(labelWidth, 28),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Microsoft Sans Serif", 11F)
            };
            var input = CreateInput(field);
            input.Location = new Point(xOffset + labelWidth + 14, y + (rowHeight - 31) / 2);
            input.Size = new Size(Math.Max(120, columnWidth - labelWidth - 32), 31);
            if (input is TextBox textBox)
            {
                textBox.MaxLength = field.MaxLength;
                textBox.ReadOnly = field.ReadOnly;
            }
            if (input is ComboBox comboBox)
                comboBox.Enabled = !field.ReadOnly;
            if (_initialValues.TryGetValue(field.Name, out var value))
                SetInputValue(input, field.Type, value, field.Choices);
            _inputs[field.Name] = input;
            body.Controls.Add(label);
            body.Controls.Add(input);
        }

        var save = new Button
        {
            Text = _saveCaption,
            BackColor = Color.Moccasin,
            Size = new Size(135, 42),
            Location = new Point(ClientSize.Width / 2 - 150, ClientSize.Height - 64),
            UseVisualStyleBackColor = false
        };
        save.Click += Save_Click;
        Controls.Add(save);

        var cancel = new Button
        {
            Text = "Thoát",
            BackColor = SystemColors.GradientActiveCaption,
            Size = new Size(135, 42),
            Location = new Point(ClientSize.Width / 2 + 15, ClientSize.Height - 64),
            UseVisualStyleBackColor = false,
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(cancel);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static Control CreateInput(EditField field)
    {
        if (field.Type == EditFieldType.Choice)
        {
            var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            if (field.Choices is not null)
                combo.Items.AddRange(field.Choices);
            return combo;
        }
        if (field.Type == EditFieldType.Date)
            return new DateTimePicker { Format = DateTimePickerFormat.Short };
        return new TextBox();
    }

    private static void SetInputValue(Control input, EditFieldType type, string value, string[]? choices)
    {
        switch (type)
        {
            case EditFieldType.Date:
                if (DateTime.TryParse(value, out var date) && input is DateTimePicker picker)
                    picker.Value = date < picker.MinDate ? picker.MinDate : date > picker.MaxDate ? picker.MaxDate : date;
                break;
            case EditFieldType.Choice:
                if (input is ComboBox combo && choices is not null)
                    combo.SelectedItem = choices.FirstOrDefault(item => string.Equals(item, value, StringComparison.OrdinalIgnoreCase));
                break;
            default:
                input.Text = value;
                break;
        }
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in _fields)
        {
            var input = _inputs[field.Name];
            var value = input switch
            {
                DateTimePicker picker => picker.Value.ToString("yyyy-MM-dd"),
                ComboBox combo => combo.SelectedItem?.ToString() ?? string.Empty,
                _ => input.Text.Trim()
            };
            if (field.Required && string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show($"Vui lòng nhập {field.Caption.ToLowerInvariant()}.", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                input.Focus();
                return;
            }
            values[field.Name] = value;
        }

        try
        {
            if (_save(values))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không lưu được thông tin.\n{ex.Message}", "Lỗi CSDL",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
