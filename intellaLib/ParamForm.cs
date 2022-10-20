using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Lib
{
	/// @ todo change parameters to a hashtable with getter/setter
	public class ParamForm : Form
	{
		/// list of parameter names passed to this form on creation
		public List<string> m_parameterNames;
		/// corresponding list of parameter values
		public List<string> m_parameterValues;

		/////////////////////////////////////////////////////////////////////////////////////
		/// delegate description for refreshList callback.
		///
		/// Used to tell child form to refreshList when asked by client via initParameters().
		public delegate void refreshCallbackType();

		private refreshCallbackType m_paramInitRefreshCallback = null;
		private refreshCallbackType m_buttonRefreshCallback = null;

		public refreshCallbackType ParamInitRefreshCallback
		{
			set
			{
				m_paramInitRefreshCallback = value;
			}
		}

		public refreshCallbackType ButtonRefreshCallback
		{
			set
			{
				m_buttonRefreshCallback = value;
			}
		}

		public void initParameters(string[] parameterNames, string[] parameterValues)
		{
			initParameters(parameterNames, parameterValues, false);
		}

		public void initParameters(string[] parameterNames, string[] parameterValues, bool doRefresh)
		{
			if (parameterNames != null && parameterValues != null)
			{
				m_parameterNames = new List<string>(parameterNames);
				m_parameterValues = new List<string>(parameterValues);
			}
			else
			{
				m_parameterNames = new List<string>(0);
				m_parameterValues = new List<string>(0);
			}
			if (doRefresh)
				paramInitRefresh();

		}

		public void buttonRefresh()
		{
			if (m_buttonRefreshCallback != null)
				m_buttonRefreshCallback();
		}

		private void paramInitRefresh()
		{
			if (m_paramInitRefreshCallback != null)
				m_paramInitRefreshCallback();
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ParamForm
			// 
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.KeyPreview = true;
			this.Name = "ParamForm";
			this.ResumeLayout(false);

		}

	}
}
