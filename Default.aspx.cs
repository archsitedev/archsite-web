﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Shufan.DB.StoreProcedureAccess;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    /*
    public void Login_OnClick(object sender, EventArgs args)
    {
        if (Membership.ValidateUser(UsernameTextbox.Text, PasswordTextbox.Text))
        {
            //FormsAuthentication.RedirectFromLoginPage(UsernameTextbox.Text, true);
            AgolToken ar = null;
            RestToken rt = null;


            Session["username"] = UsernameTextbox.Text;
            String[] roles = Roles.GetRolesForUser(UsernameTextbox.Text);

            MembershipUser mu = Membership.GetUser(UsernameTextbox.Text);

            if (roles.Length > 0)//logined user
            {
                //shufan added codes 08082014======
                try
                {
                    string strSQLConn = System.Configuration.ConfigurationManager.AppSettings["SQLConString"].Trim();

                    //insert log information
                    string[] strParas = new string[2];
                    strParas[0] = UsernameTextbox.Text;
                    strParas[1] = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");

                    clsStoreProcedureAccess clsInsertLog = new clsStoreProcedureAccess("InsertLogRec", strSQLConn);
                    clsInsertLog.fnExecuteSP(strParas);
                }
                catch (Exception ex)
                {
                    throw ex;
                }//Shufan added codes end


                string url = "https://www.arcgis.com/sharing/oauth2/token?" +
                                  "client_id=" + ConfigurationManager.AppSettings["client_id"] +
                                  "&client_secret=" + ConfigurationManager.AppSettings["client_secret"] +
                                  "&grant_type=client_credentials";


                ar = MakeRequest(url);

                if (ar != null)
                {
                    Session["agol_token"] = ar.access_token;

                }

                //url = "https://www.arcgis.com/sharing/generateToken?f=json&" +
                //                  "&token=" + ar.access_token + 
                //                  "&serverUrl=http://analysis.arcgis.com";


                url = "https://www.arcgis.com/sharing/generateToken?f=json&" +
                                  "&username=" + ConfigurationManager.AppSettings["agol_user"] +
                                  "&password=" + ConfigurationManager.AppSettings["agol_password"] +
                                  "&referer=http://www.arcgis.com";

                rt = MakeRequest2(url);

                if (rt != null)
                {
                    Session["agol_analysis_token"] = rt.token;
                }

                Session["rolename"] = roles[0];
                Session["DISPLAYNAME"] = "";
                Session["AFFILIATION"] = "";

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString);

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "[dbo].[aspnet_GetUserInfo]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 256).Value = UsernameTextbox.Text;

                SqlCommand iCmd = new SqlCommand();
                iCmd.CommandText = "[dbo].[aspnet_InsertUserToken]";
                iCmd.CommandType = CommandType.StoredProcedure;
                iCmd.Connection = conn;

                iCmd.Parameters.Add("@username", SqlDbType.NVarChar, 256).Value = UsernameTextbox.Text;
                iCmd.Parameters.Add("@token", SqlDbType.NVarChar, 256).Value = ar.access_token;
                System.TimeSpan duration = new System.TimeSpan(0, 0, 0, ar.expires_in);
                iCmd.Parameters.Add("@expired", SqlDbType.DateTime).Value = DateTime.Now.Add(duration);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String name = "";
                        if (!(reader["FIRSTNAME"] is System.DBNull))
                            name += reader["FIRSTNAME"].ToString();
                        if (!(reader["LASTNAME"] is System.DBNull))
                            name += " " + reader["LASTNAME"].ToString();

                        Session["DISPLAYNAME"] = name;

                        String affil = "";
                        if (!(reader["AFFILIATION"] is System.DBNull))
                            affil = reader["AFFILIATION"].ToString();

                        Session["AFFILIATION"] = affil;
                    }
                }

                //Insert the token
                iCmd.ExecuteNonQuery();

                conn.Close();

            }
            if (mu.Comment == "cp")
            {
                Response.Redirect("ChangePassword.aspx");
            }
            else if (Roles.GetRolesForUser(mu.UserName)[0].Equals("admin"))
            {
                Response.Redirect("~/ManagementTool.aspx");
            }
            else
            {
                Response.Redirect("~/Map/Map.aspx");
            }


        }
    }
    */

    public AgolToken MakeRequest(string requestUrl)
    {
        try
        {
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(AgolToken));
                AgolToken jsonResponse = jsonSerializer.ReadObject(response.GetResponseStream()) as AgolToken;
                return jsonResponse;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            //throw new Exception(e.StackTrace);
            Response.Redirect("~/Default.aspx?error=" + e.Message);
            return null;
        }
    }

    public RestToken MakeRequest2(string requestUrl)
    {
        try
        {
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RestToken));
                RestToken jsonResponse = jsonSerializer.ReadObject(response.GetResponseStream()) as RestToken;

                //Stream dataStream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //string responseFromServer = reader.ReadToEnd();
                ////Console.WriteLine(responseFromServer);
                //reader.Close();
                //response.Close();

                return jsonResponse;

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            //throw new Exception(e.StackTrace);
            //Response.Redirect("~/Default.aspx?error=" + e.Message);
            return null;
        }
    }

    protected void Login1_LoggedIn(object sender, EventArgs e)
    {
        AgolToken ar = null;
        RestToken rt = null;


        Session["username"] = Login1.UserName;
        String[] roles = Roles.GetRolesForUser(Login1.UserName);

        MembershipUser mu = Membership.GetUser(Login1.UserName);

        if (roles.Length > 0)
        {
            //shufan added codes 08082014======
            try
            {
                string strSQLConn = System.Configuration.ConfigurationManager.AppSettings["SQLConString"].Trim();

                //insert log information
                string[] strParas = new string[2];
                strParas[0] = Login1.UserName;
                strParas[1] = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");

                clsStoreProcedureAccess clsInsertLog = new clsStoreProcedureAccess("InsertLogRec", strSQLConn);
                clsInsertLog.fnExecuteSP(strParas);
            }
            catch (Exception ex)
            {
                throw ex;
            }//Shufan added codes end

            if (Roles.GetRolesForUser(mu.UserName).Contains("pending") == false)
            {

                string url = "https://www.arcgis.com/sharing/oauth2/token?" +
                                  "client_id=" + ConfigurationManager.AppSettings["client_id"] +
                                  "&client_secret=" + ConfigurationManager.AppSettings["client_secret"] +
                                  "&grant_type=client_credentials";


                ar = MakeRequest(url);

                if (ar != null)
                {
                    Session["agol_token"] = ar.access_token;

                }

                //url = "https://www.arcgis.com/sharing/generateToken?f=json&" +
                //                  "&token=" + ar.access_token + 
                //                  "&serverUrl=http://analysis.arcgis.com";


                url = "https://www.arcgis.com/sharing/generateToken?f=json&" +
                                  "&username=" + ConfigurationManager.AppSettings["agol_user"] +
                                  "&password=" + ConfigurationManager.AppSettings["agol_password"] +
                                  "&referer=https://www.scarchsite.org";

                rt = MakeRequest2(url);

                if (rt != null)
                {
                    Session["agol_analysis_token"] = rt.token;
                }

                Session["rolename"] = roles[0];
                Session["DISPLAYNAME"] = "";
                Session["AFFILIATION"] = "";

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString);

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "[dbo].[aspnet_GetUserInfo]";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;

                cmd.Parameters.Add("@username", SqlDbType.NVarChar, 256).Value = Login1.UserName;

                SqlCommand iCmd = new SqlCommand();
                iCmd.CommandText = "[dbo].[aspnet_InsertUserToken]";
                iCmd.CommandType = CommandType.StoredProcedure;
                iCmd.Connection = conn;

                iCmd.Parameters.Add("@username", SqlDbType.NVarChar, 256).Value = Login1.UserName;
                iCmd.Parameters.Add("@token", SqlDbType.NVarChar, 256).Value = ar.access_token;
                System.TimeSpan duration = new System.TimeSpan(0, 0, 0, ar.expires_in);
                iCmd.Parameters.Add("@expired", SqlDbType.DateTime).Value = DateTime.Now.Add(duration);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        String name = "";
                        if (!(reader["FIRSTNAME"] is System.DBNull))
                            name += reader["FIRSTNAME"].ToString();
                        if (!(reader["LASTNAME"] is System.DBNull))
                            name += " " + reader["LASTNAME"].ToString();

                        Session["DISPLAYNAME"] = name;

                        String affil = "";
                        if (!(reader["AFFILIATION"] is System.DBNull))
                            affil = reader["AFFILIATION"].ToString();

                        Session["AFFILIATION"] = affil;
                    }
                }

                //Insert the token
                iCmd.ExecuteNonQuery();

                conn.Close();

            }

        }

        if (mu.Comment == "cp")
        {
            Response.Redirect("ChangePassword.aspx");
        }
        else if (Roles.GetRolesForUser(mu.UserName)[0].Equals("admin"))
        {
            Response.Redirect("~/ManagementTool.aspx");
        }
        else if (Roles.GetRolesForUser(mu.UserName).Contains("pending") == false)
        {
            Response.Redirect("~/Map/Map.aspx");
        }
        else
        {
            Response.Redirect("~/PublicView.aspx");
        }
    }
}