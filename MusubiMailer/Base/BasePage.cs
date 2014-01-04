using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using DAL;

namespace MusubiMailer
{
    public partial class BasePage : Page
    {
        protected IMusubiRepository _musubiRepo = new MusubiSQLRepository();
    }
}
