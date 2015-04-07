using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace UserBehaviorAnalyst
{
    public struct TripRecord
    {
        public int userId;
        public DateTime boardingTime;
        public DateTime alightTime;
        public string busLine;
    }

    public static class DataOperation
    {
        private static OleDbConnection oleCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=UserBehavior.mdb");
        private static OleDbCommand oleComd;
        private static OleDbDataAdapter adpt = new OleDbDataAdapter();

        public static void InsertData(TripRecord record)
        {
            string command = "INSERT INTO TripRecord (CellPhoneAddr,BoardTime,AlightTime,BusLine) VALUES("+record.userId +",'"+record.boardingTime+"','"+record.alightTime+"','" +record.busLine+"')";
            oleComd = new OleDbCommand(command, oleCon);
            try
            {
                //oleCon.Open();
                adpt.InsertCommand = oleComd;
                adpt.InsertCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message+"\n"+command);
            }
        }

        public static void OpenConnection()
        {
            oleCon.Open();
        }

        public static void CloseConnection()
        {
            oleCon.Close();
        }

        public static void ClearData()
        {
            string command = "DELETE FROM TripRecord";
            oleComd = new OleDbCommand(command, oleCon);
            try
            {
                oleCon.Open();
                adpt.DeleteCommand = oleComd;
                adpt.DeleteCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message + "\n" + command);
            }
            finally
            {
                oleCon.Close();
            }
        }

        public static DataSet RetrieveData(string table)
        {
            string command = "SELECT * FROM "+ table;
            oleComd = new OleDbCommand(command, oleCon);
            DataSet dtst = new DataSet();
            try
            {
                oleCon.Open();
                adpt.SelectCommand = oleComd;
                adpt.Fill(dtst, table);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message + "\n" + command);
            }
            finally
            {
                oleCon.Close();
            }
            return dtst;
        }
    }
}
