using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace QueueLib
{

	public partial class PasswordDialogForm : System.Windows.Forms.Form
	{
		private PasswordValidateCallback m_passwordValidateCallback;

		private PasswordSuccessCallback m_passwordSuccessCallback;
		public delegate bool PasswordValidateCallback(string password);
		public delegate void PasswordSuccessCallback();

		private int m_trysDefault = 3;

		private int m_trys = 0;
		private void Form_Show()
		{
			m_trys = m_trysDefault;
		}

		public void SetValidateCallback(PasswordValidateCallback callback)
		{
			m_passwordValidateCallback = callback;
		}

		public void SetSuccessCallback(PasswordSuccessCallback callback)
		{
			m_passwordSuccessCallback = callback;
		}
		
		private void TextBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			// Enter key
			if (e.KeyChar == Strings.Chr(13))
			{
				if (1==1) //(m_passwordValidateCallback.Invoke(this.TextBox_Password.Text))
				{
					this.Close();
					m_passwordSuccessCallback.Invoke();
				}
				else
				{
					m_trys = m_trys - 1;
					this.TextBox_Password.Text = "";

					if ((m_trys == 0))
					{
						this.Close();
					}
				}
			}
		}
		
		public PasswordDialogForm()
		{
//			Shown += Form_Show;
			// required by the designer.
			InitializeComponent();

			// Add any initialization after the InitializeComponent() call.
			this.TopMost = true;
		}

		private void PasswordDialogForm_Shown(object sender, EventArgs e)
		{
			Form_Show();
		}

        private void PasswordDialogForm_Load(object sender, EventArgs e) {

        }

        private void TextBox_Password_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
