using System;
using System.Collections.Generic;
using System.Text;

namespace Lib
{
    class Opts {
        KeyValuePair<string, string> m_opts = new KeyValuePair<string, string>();

        public Opts() {

        }

        public Opts(KeyValuePair<string, string> opts) {
            m_opts = opts;
        }
    }
}
