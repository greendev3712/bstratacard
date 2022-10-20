using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Npgsql;
using System.Windows.Forms;

namespace QueueLib
{

    //public class SimpleDB
    //{
    //    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    //    private NpgsqlConnection PostgresConnection;

    //    private bool PostgresConnectionSuccessful = false;
    //    private string DB_Host;
    //    private string DB_Port;
    //    private string DB_User;
    //    private string DB_Pass;

    //    private string DB_Name;

    //    private string DB_Error;
    //    public void SetConnection(string host, string port, string user, string pass, string name)
    //    {
    //        DB_Host = host;
    //        DB_Port = port;
    //        DB_User = user;
    //        DB_Pass = pass;
    //        DB_Name = name;
    //    }

    //    public NpgsqlConnection NpgsqlConnection
    //    {
    //        get
    //        {
    //            return PostgresConnection;
    //        }
    //    }

    //    public bool IsConnected()
    //    {
    //        return PostgresConnectionSuccessful;
    //    }

    //    public object GetError()
    //    {
    //        return DB_Error;
    //    }

    //    public bool Connect()
    //    {
    //        string dsn = null;
    //        dsn = "Server=" + DB_Host + ";";
    //        dsn += "Port=" + DB_Port + ";";
    //        dsn += "User Id=" + DB_User + ";";
    //        dsn += "Password=" + DB_Pass + ";";
    //        dsn += "Database=" + DB_Name + ";";

    //        try
    //        {
    //            PostgresConnection = new NpgsqlConnection(dsn);
    //            PostgresConnection.Open();
    //        }
    //        catch (Exception ex)
    //        {
    //            DB_Error = ex.Message;
    //            PostgresConnectionSuccessful = false;
    //            return false;
    //        }

    //        PostgresConnectionSuccessful = true;
    //        return true;
    //    }

    //    ///'''''''''''''''''''''''''''''''''''''''''''''''
    //    // Do A query
    //    //  (since VB can't easily return null or false,
    //    //   we return an ArrayList so the caller can
    //    //   easily check the result)
    //    //
    //    // queryString is executed
    //    // Returns an ArrayList
    //    //
    //    // On error: array[0] = "failure"
    //    // On success: array[0] = "success"
    //    //             array[1] = NpgsqlDataReader
    //    //
    //    public ArrayList Query(string queryString)
    //    {
    //        //object functionReturnValue = null;
    //        NpgsqlCommand m_cmd = default(NpgsqlCommand);
    //        NpgsqlDataReader m_reader = default(NpgsqlDataReader);
    //        ArrayList m_result = new ArrayList();

    //        // See if we're still connected.  If not, reconnect
    //        try
    //        {
    //            log.Info("SQL: SELECT 1");
    //            m_cmd = new NpgsqlCommand("SELECT 1", PostgresConnection);
    //            m_reader = m_cmd.ExecuteReader();
    //            m_reader.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            if (Connect() != true)
    //            {
    //                PostgresConnectionSuccessful = false;
    //                DB_Error = ex.Message;
    //                m_result.Add("failure");
    //                return m_result;
    //            }
    //        }

    //        try
    //        {
    //            log.Info("SQL: " + queryString);
    //            m_cmd = new NpgsqlCommand(queryString, PostgresConnection);
    //            m_reader = m_cmd.ExecuteReader();
    //        }
    //        catch (Exception ex)
    //        {
    //            DB_Error = ex.Message;
    //            PostgresConnection.Close();
    //            m_result.Add("failure");

    //            return m_result;
    //        }

    //        m_result.Add("success");
    //        m_result.Add(m_reader);

    //        return m_result;
    //    }

    //    public bool executeSimpleQuery(string query)
    //    {
    //        NpgsqlDataReader reader = default(NpgsqlDataReader);
    //        ArrayList result = null;

    //        result = Query(query);
    //        if ((string)result[0] == "failure")
    //        {
    //            //ProgramError();
    //            Debug.Print(query + " failed");
    //            MessageBox.Show("Exception: " + query + ": " + GetError(), "Error");
    //            return false;
    //        }

    //        reader = (NpgsqlDataReader)result[1];

    //        try
    //        {
    //            reader.Read();
    //        }
    //        catch (Exception ex)
    //        {
    //            //ProgramError();
    //            Debug.Print(query + " processing failed");
    //            MessageBox.Show("Exception: " + query + ": " + ex.ToString(), "Error");
    //            return false;
    //        }
    //        finally
    //        {
    //            reader.Close();
    //        }
    //        return true;
    //    }

    //    public ArrayList SelectHash(string queryString)
    //    {
    //        NpgsqlDataReader reader = default(NpgsqlDataReader);
    //        ArrayList query_result = null;
    //        ArrayList return_result = new ArrayList();

    //        query_result = this.Query(queryString);

    //        if ((string)query_result[0] == "failure")
    //        {
    //            Debug.Print("SimpleDB.SelectArray() Failed.  Query: " + queryString);
    //            Debug.Print("Exception: " + this.GetError());
    //            MessageBox.Show("Exception: " + this.GetError(), "Error");
    //            throw new Exception("Query Exception. " + this.GetError());
    //        }

    //        try
    //        {
    //            reader = (NpgsqlDataReader)query_result[1];

    //            while (reader.Read())
    //            {
    //                int num_fields = reader.FieldCount;
    //                Hashtable row_data = new Hashtable();

    //                for (int field_num = 0; field_num <= (num_fields - 1); field_num++)
    //                {
    //                    string field_name = reader.GetName(field_num);
    //                    string newValue;

    //                    try
    //                    {
    //                        newValue = this.GetColumn(reader, field_name);
    //                        if (!row_data.ContainsKey(field_name))
    //                        {
    //                            row_data.Add(field_name, newValue);
    //                        }
    //                        else if ((string)row_data[field_name] != newValue)
    //                        {
    //                            Debug.Print("Warning: Column " + field_name + " already exists. Changing value to " + newValue + ".");
    //                            row_data[field_name] = newValue;
    //                        }
    //                        else
    //                        {
    //                            //Debug.Print("Warning: Column " + field_name + " already exists, but value is the same: " + newValue + ".");
    //                        }
    //                    }
    //                    catch (ArgumentException ex)
    //                    {
    //                        // this should never happen
    //                        Debug.Print("Exception: " + ex.Message);
    //                    }
    //                }

    //                return_result.Add(row_data);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Debug.Print("SimpleDB.SelectArray() Failed to read a row.  Query: " + queryString);
    //            Debug.Print("Exception: " + ex.Message);
    //            MessageBox.Show("Exception: " + ex.Message, "Error");
    //            throw new Exception("Query Exception. " + ex.Message);
    //        }

    //        reader.Close();

    //        return return_result;
    //    }

    //    public string GetColumn(NpgsqlDataReader dbReader, string columnName)
    //    {
    //        string column_value = "";

    //        try
    //        {
    //            column_value = dbReader[columnName].ToString();
    //        }
    //        catch (Exception ex)
    //        {
    //            // Either the column doesn't exist or the cast failed
    //            // We'll treat this as an empty string
    //            Debug.Print("Exception: " + ex.Message);
    //        }

    //        return column_value;
    //    }
    //}
}