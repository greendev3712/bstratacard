using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Lib {
    public partial class LiveDataGridViewForm : Form {
        public LiveDataGridViewForm() {
            InitializeComponent();
        }

        public LiveDataGridView GetLiveDataGrid() {
            return this.cmpLiveDataGridView;
        }
    }
}
