using System.Windows.Forms;
using SerialNumberOCR.Forms;

namespace SerialNumberOCR;

public class Program
{
    [STAThread]
    public static void Main()
    {
       Application.EnableVisualStyles();
       Application.SetCompatibleTextRenderingDefault(false);
       Application.Run(new MainForm());
    }
    
    
}