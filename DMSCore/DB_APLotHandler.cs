using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

using System.Data.SqlTypes;
using System.Data.Common;

namespace MonitorCore
{
    class DB_APLotHandler
    {
        private SqlConnection sqlCnt = null;
        private SqlCommand command_APHandler = null;
        private bool bStored_Init;
        private int cmd_enable;
        private int identity;
        public DB_APLotHandler(int cmd_enable, int identity)
        {
            sqlCnt = new SqlConnection();

            command_APHandler = new SqlCommand();
            command_APHandler.CommandType = CommandType.StoredProcedure;
            this.identity = identity;
            this.cmd_enable = cmd_enable;
        }

        #region Open database
        public bool Open(string ServerName, string account, string password, string database, int timeout = 5)
        {
            try
            {
                string cntStr = "server=" + ServerName + ";user id=" + account + ";password=" + password +
                    ";MultipleActiveResultSets=true;database=" + database + ";Connection Timeout=" + timeout.ToString() + ";";
                sqlCnt.ConnectionString = cntStr;

                sqlCnt.Open();

                Thread.Sleep(500);
                if (ConnectionState.Open == sqlCnt.State)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (InvalidOperationException ex)
            {
                Log.write("DB_APLotHandler---Open:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("DB_APLotHandler---Open:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("DB_APLotHandler---Open:Error-" + "Message: " + ex.Message,0);
                return false;
            }


        }
        #endregion

        #region Close database
        public bool Close(int timeout = 3000)
        {

            sqlCnt.Close();
            return true;
        }
        #endregion

        #region Operation
        public bool AP_Show_Msg(string Label,ref string SO_NO,ref string COLOR_NO,ref string Lot_NO,ref string ClothName,ref string Cloth_Kind)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_AP_SHOW_MSG";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@label", SqlDbType.VarChar, 50).Value = Label;

                //返回参数设置
                SqlParameter Ret_SO_NO = new SqlParameter("@SO_NO", SqlDbType.VarChar,50);
                Ret_SO_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_SO_NO);
                //返回参数设置
                SqlParameter Ret_COLOR_NO = new SqlParameter("@COLOR_NO", SqlDbType.VarChar, 50);
                Ret_COLOR_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COLOR_NO);
                //返回参数设置
                SqlParameter Ret_Lot_NO = new SqlParameter("@Lot_NO", SqlDbType.VarChar, 50);
                Ret_Lot_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_Lot_NO);
                //返回参数设置
                SqlParameter Ret_ClothName = new SqlParameter("@ClothName", SqlDbType.VarChar, 50);
                Ret_ClothName.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_ClothName);
                //返回参数设置
                SqlParameter Ret_Cloth_Kind = new SqlParameter("@Cloth_Kind", SqlDbType.VarChar, 50);
                Ret_Cloth_Kind.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_Cloth_Kind);
                //返回值设置
                SqlParameter parReturn = new SqlParameter("@return", SqlDbType.VarChar, 50);
                parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);
                //执行
                command_APHandler.ExecuteNonQuery();

                SO_NO = Ret_SO_NO.Value.ToString();
                COLOR_NO = Ret_COLOR_NO.Value.ToString();
                Lot_NO = Ret_Lot_NO.Value.ToString();
                ClothName = Ret_ClothName.Value.ToString();
                Cloth_Kind = Ret_Cloth_Kind.Value.ToString();
                //返回值
                if (parReturn.Value.Equals(1))
                {
                    Log.write("Database " + " --AP_Show_Msg:TRUE");
                    return true;
                }
                else
                {
                    Log.write("Database " + " --AP_Show_Msg:FALSE");
                    return false;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + " --AP_Show_Msg:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + " --AP_Show_Msg:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + " --AP_Show_Msg:Error-" + "Message: " + ex.Message,0);
                return false;
            }
        }
        public byte AP_Lot_Add(string Label, string LotNo)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_AP_LOT_ADD";
                bStored_Init = true;

                //参数添加
                command_APHandler.Parameters.Add("@label", SqlDbType.VarChar, 50).Value = Label;

                //参数添加
                command_APHandler.Parameters.Add("@LotNo", SqlDbType.VarChar, 50).Value = LotNo;

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);
                //执行
                command_APHandler.ExecuteNonQuery();

                //返回值
                byte ret = byte.Parse(parReturn.Value.ToString());

                if (parReturn.Value.Equals(1))
                {
                    Log.write("Database " + " --AP_Lot_Add:TRUE");
                    return ret;
                }
                else
                {
                    Log.write("Database " + " --AP_Lot_Add:FALSE");
                    return ret;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + " --AP_Lot_Add:OperationError-" + "Message: " + ex.Message,0);
                return 5;
            }
            catch (SqlException ex)
            {
                Log.write("Database " +  " --AP_Lot_Add:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return 5;
            }
            catch (Exception ex)
            {
                Log.write("Database " + " --AP_Lot_Add:Error-" + "Message: " + ex.Message,0);
                return 5;
            }
        }

        public byte AP_Lot_Decrease(string Label, string LotNo)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_AP_LOT_DECREASE";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@label", SqlDbType.VarChar, 50).Value = Label;

                //参数添加 ap 通道数
                command_APHandler.Parameters.Add("@LotNo", SqlDbType.VarChar, 50).Value = LotNo;

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@return", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.ReturnValue; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);
                //执行
                command_APHandler.ExecuteNonQuery();

                //返回值
                byte ret = byte.Parse(parReturn.Value.ToString());
                if (parReturn.Value.Equals(1))
                {
                    Log.write("Database " + " --AP_Lot_Add:TRUE");
                    return ret;
                }
                else
                {
                    Log.write("Database " + " --AP_Lot_Add:FALSE");
                    return ret;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + " --AP_Lot_Add:OperationError-" + "Message: " + ex.Message,0);
                return 5;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + " --AP_Lot_Add:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return 5;
            }
            catch (Exception ex)
            {
                Log.write("Database " + " --AP_Validate:Error-" + "Message: " + ex.Message,0);
                return 5;
            }
        }

        public int Mobile_Info_Get(string str_SONO, ref string COLOR_NO, ref string strPos, ref string ClothName, ref string Cloth_Kind)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_MOBILE_MSG_GET";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@SO_NO", SqlDbType.VarChar, 20).Value = str_SONO;

                //返回参数设置BATCH_DATE
                SqlParameter Ret_BATCH_DATE = new SqlParameter("@BATCH_DATE", SqlDbType.DateTime);
                Ret_BATCH_DATE.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_BATCH_DATE);

                //返回参数设置DOC_TYPE_OUT
                SqlParameter Ret_DOC_TYPE_OUT = new SqlParameter("@DOC_TYPE_OUT", SqlDbType.VarChar, 10);
                Ret_DOC_TYPE_OUT.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_DOC_TYPE_OUT);

                //返回参数设置SEQ_NO
                SqlParameter Ret_SEQ_NO = new SqlParameter("@SEQ_NO", SqlDbType.VarChar, 20);
                Ret_SEQ_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_SEQ_NO);

                //返回参数设置COL_NO
                SqlParameter Ret_COL_NO = new SqlParameter("@COL_NO", SqlDbType.VarChar, 20);
                Ret_COL_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO);


                //返回参数设置LOT_NO
                SqlParameter Ret_LOT_NO = new SqlParameter("@LOT_NO", SqlDbType.VarChar, 20);
                Ret_LOT_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_LOT_NO);

                //返回参数设置COL_NO_OK
                SqlParameter Ret_COL_NO_OK = new SqlParameter("@COL_NO_OK", SqlDbType.VarChar, 20);
                Ret_COL_NO_OK.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_OK);
                //返回参数设置CUST_NO
                SqlParameter Ret_CUST_NO = new SqlParameter("@CUST_NO", SqlDbType.VarChar, 20);
                Ret_CUST_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_CUST_NO);
                //返回参数设置CUST_SHORT
                SqlParameter Ret_CUST_SHORT = new SqlParameter("@CUST_SHORT", SqlDbType.VarChar, 20);
                Ret_CUST_SHORT.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_CUST_SHORT);
                //返回参数设置PROD_NO
                SqlParameter Ret_PROD_NO = new SqlParameter("@PROD_NO", SqlDbType.VarChar, 20);
                Ret_PROD_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_NO);
                //返回参数设置PROD_NAME2
                SqlParameter Ret_PROD_NAME2 = new SqlParameter("@PROD_NAME2", SqlDbType.VarChar, 20);
                Ret_PROD_NAME2.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_NAME2);
                //返回参数设置PROD_TYPE
                SqlParameter Ret_PROD_TYPE = new SqlParameter("@PROD_TYPE", SqlDbType.VarChar, 20);
                Ret_PROD_TYPE.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_TYPE);
                //返回参数设置COL_NAME1
                SqlParameter Ret_COL_NAME1 = new SqlParameter("@COL_NAME1", SqlDbType.VarChar, 50);
                Ret_COL_NAME1.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NAME1);

                //返回参数设置COL_NAME2
                SqlParameter Ret_COL_NAME2 = new SqlParameter("@COL_NAME2", SqlDbType.VarChar, 50);
                Ret_COL_NAME2.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NAME2);

                //返回参数设置COL_NO_C
                SqlParameter Ret_COL_NO_C = new SqlParameter("@COL_NO_C", SqlDbType.VarChar, 50);
                Ret_COL_NO_C.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_C);

                //返回参数设置COL_NO_L 
                SqlParameter Ret_COL_NO_L = new SqlParameter("@COL_NO_L", SqlDbType.VarChar, 50);
                Ret_COL_NO_L.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_L);

                //返回参数设置LBL_S
                SqlParameter Ret_LBL_S = new SqlParameter("@LBL_S", SqlDbType.VarChar, 100);
                Ret_LBL_S.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_LBL_S);

                //返回参数设置BUYER
                SqlParameter Ret_BUYER = new SqlParameter("@BUYER", SqlDbType.VarChar, 100);
                Ret_BUYER.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_BUYER);

                //返回参数设置REM
                SqlParameter Ret_REM = new SqlParameter("@REM", SqlDbType.VarChar,100);
                Ret_REM.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_REM);

                //返回参数设置Ret_POS
                SqlParameter Ret_POS = new SqlParameter("@POS", SqlDbType.VarChar, 100);
                Ret_POS.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_POS);

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@OK", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);

                //返回值设置message
                SqlParameter parmessage = new SqlParameter("@message", SqlDbType.VarChar,200);
                parmessage.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parmessage);

                //执行
                command_APHandler.ExecuteNonQuery();
                COLOR_NO = Ret_COL_NO.Value.ToString();
                ClothName = Ret_PROD_NAME2.Value.ToString();
                Cloth_Kind = Ret_PROD_TYPE.Value.ToString();
                strPos = Ret_POS.Value.ToString();
                //返回值
                byte ret = byte.Parse(parReturn.Value.ToString());
                if (ret==1)
                {
                    Log.write("Database " + " --Mobile_Info_Get:TRUE");
                    return ret;
                }
                else
                {
                    Log.write("Database " + " --Mobile_Info_Get:FALSE");
                    return ret;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:OperationError-" + "Message: " + ex.Message, 0);
                return 5;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return 5;
            }
            catch (Exception ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:Error-" + "Message: " + ex.Message, 0);
                return 5;
            }
        }

        public int Mobile_Info_Get_By_RFID(string strRFID, ref string strSONO, ref string COLOR_NO, ref string strPos, ref string ClothName, ref string Cloth_Kind)
        {
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_MOBILE_MSG_GET_BY_RFID";
                bStored_Init = true;

                //参数添加 ap uid
                command_APHandler.Parameters.Add("@RFID_NO", SqlDbType.VarChar, 20).Value = strRFID;

                //返回参数设置BATCH_DATE
                SqlParameter Ret_SO_NO = new SqlParameter("@SO_NO", SqlDbType.VarChar, 50);
                Ret_SO_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_SO_NO);

                //返回参数设置BATCH_DATE
                SqlParameter Ret_BATCH_DATE = new SqlParameter("@BATCH_DATE", SqlDbType.DateTime);
                Ret_BATCH_DATE.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_BATCH_DATE);

                //返回参数设置DOC_TYPE_OUT
                SqlParameter Ret_DOC_TYPE_OUT = new SqlParameter("@DOC_TYPE_OUT", SqlDbType.VarChar, 10);
                Ret_DOC_TYPE_OUT.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_DOC_TYPE_OUT);

                //返回参数设置SEQ_NO
                SqlParameter Ret_SEQ_NO = new SqlParameter("@SEQ_NO", SqlDbType.VarChar, 20);
                Ret_SEQ_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_SEQ_NO);

                //返回参数设置COL_NO
                SqlParameter Ret_COL_NO = new SqlParameter("@COL_NO", SqlDbType.VarChar, 20);
                Ret_COL_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO);


                //返回参数设置LOT_NO
                SqlParameter Ret_LOT_NO = new SqlParameter("@LOT_NO", SqlDbType.VarChar, 20);
                Ret_LOT_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_LOT_NO);

                //返回参数设置COL_NO_OK
                SqlParameter Ret_COL_NO_OK = new SqlParameter("@COL_NO_OK", SqlDbType.VarChar, 20);
                Ret_COL_NO_OK.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_OK);
                //返回参数设置CUST_NO
                SqlParameter Ret_CUST_NO = new SqlParameter("@CUST_NO", SqlDbType.VarChar, 20);
                Ret_CUST_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_CUST_NO);
                //返回参数设置CUST_SHORT
                SqlParameter Ret_CUST_SHORT = new SqlParameter("@CUST_SHORT", SqlDbType.VarChar, 20);
                Ret_CUST_SHORT.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_CUST_SHORT);
                //返回参数设置PROD_NO
                SqlParameter Ret_PROD_NO = new SqlParameter("@PROD_NO", SqlDbType.VarChar, 20);
                Ret_PROD_NO.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_NO);
                //返回参数设置PROD_NAME2
                SqlParameter Ret_PROD_NAME2 = new SqlParameter("@PROD_NAME2", SqlDbType.VarChar, 20);
                Ret_PROD_NAME2.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_NAME2);
                //返回参数设置PROD_TYPE
                SqlParameter Ret_PROD_TYPE = new SqlParameter("@PROD_TYPE", SqlDbType.VarChar, 20);
                Ret_PROD_TYPE.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_PROD_TYPE);
                //返回参数设置COL_NAME1
                SqlParameter Ret_COL_NAME1 = new SqlParameter("@COL_NAME1", SqlDbType.VarChar, 50);
                Ret_COL_NAME1.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NAME1);

                //返回参数设置COL_NAME2
                SqlParameter Ret_COL_NAME2 = new SqlParameter("@COL_NAME2", SqlDbType.VarChar, 50);
                Ret_COL_NAME2.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NAME2);

                //返回参数设置COL_NO_C
                SqlParameter Ret_COL_NO_C = new SqlParameter("@COL_NO_C", SqlDbType.VarChar, 50);
                Ret_COL_NO_C.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_C);

                //返回参数设置COL_NO_L 
                SqlParameter Ret_COL_NO_L = new SqlParameter("@COL_NO_L", SqlDbType.VarChar, 50);
                Ret_COL_NO_L.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_COL_NO_L);

                //返回参数设置LBL_S
                SqlParameter Ret_LBL_S = new SqlParameter("@LBL_S", SqlDbType.VarChar, 100);
                Ret_LBL_S.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_LBL_S);

                //返回参数设置BUYER
                SqlParameter Ret_BUYER = new SqlParameter("@BUYER", SqlDbType.VarChar, 100);
                Ret_BUYER.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_BUYER);

                //返回参数设置REM
                SqlParameter Ret_REM = new SqlParameter("@REM", SqlDbType.VarChar, 100);
                Ret_REM.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_REM);

                //返回参数设置Ret_POS
                SqlParameter Ret_POS = new SqlParameter("@POS", SqlDbType.VarChar, 100);
                Ret_POS.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(Ret_POS);

                //返回值设置
                SqlParameter parReturn = new SqlParameter("@OK", SqlDbType.Int);
                parReturn.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parReturn);

                //返回值设置message
                SqlParameter parmessage = new SqlParameter("@message", SqlDbType.VarChar, 200);
                parmessage.Direction = ParameterDirection.Output; 　　//参数类型为ReturnValue                     
                command_APHandler.Parameters.Add(parmessage);

                //执行
                command_APHandler.ExecuteNonQuery();

                strSONO = Ret_SO_NO.Value.ToString();
                COLOR_NO = Ret_COL_NO.Value.ToString();
                ClothName = Ret_PROD_NAME2.Value.ToString();
                Cloth_Kind = Ret_PROD_TYPE.Value.ToString();
                strPos = Ret_POS.Value.ToString();
                //返回值
                byte ret = byte.Parse(parReturn.Value.ToString());
                if (ret == 1)
                {
                    Log.write("Database " + " --Mobile_Info_Get:TRUE");
                    return ret;
                }
                else
                {
                    Log.write("Database " + " --Mobile_Info_Get:FALSE");
                    return ret;
                }
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:OperationError-" + "Message: " + ex.Message, 0);
                return 5;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message, 0);
                SqlExceptionHelper.Deal(ex);
                return 5;
            }
            catch (Exception ex)
            {
                Log.write("Database " + " --Mobile_Info_Get:Error-" + "Message: " + ex.Message, 0);
                return 5;
            }
        }
        #endregion

        
        public bool CmdLog(string cmdCode, string apuid, string receiveData)
        {
            if (cmd_enable != 1)
            {
                return true;
            }
            try
            {
                //命令设置
                command_APHandler = sqlCnt.CreateCommand();
                command_APHandler.CommandType = CommandType.StoredProcedure;
                command_APHandler.CommandText = "P_CMD_LOG";


                command_APHandler.Parameters.Add("@cmdCode", SqlDbType.VarChar, 20).Value = cmdCode;


                command_APHandler.Parameters.Add("@apuid", SqlDbType.VarChar, 50).Value = apuid;
                command_APHandler.Parameters.Add("@receiveData", SqlDbType.VarChar, 500).Value = receiveData;

                //执行
                int iRet = command_APHandler.ExecuteNonQuery();

                //返回值
                if (iRet == 0 || iRet == -1)
                {
                    Log.write("Database " + identity.ToString() + " --CmdLog:NOT EFFECTIVE");
                    return false;
                }

                Log.write("Database " + identity.ToString() + " --CmdLog:OK");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:OperationError-" + "Message: " + ex.Message,0);
                return false;
            }
            catch (SqlException ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:SqlError-Number:" + ex.Number.ToString() + ";Message: " + ex.Message,0);
                SqlExceptionHelper.Deal(ex);
                return false;
            }
            catch (Exception ex)
            {
                Log.write("Database " + identity.ToString() + " --CmdLog:Error-" + "Message: " + ex.Message,0);
                return false;
            }

        }
    }
}
