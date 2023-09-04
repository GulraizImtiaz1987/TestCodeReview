using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using WCS;

namespace ConveyorProcessLogicTest
{
  public class TestMethod
  {
    public ApiService api;
    readonly ConnectionInfo ci = new ConnectionInfo();
    private int systemID;
    public ConveyorProceesLogic Conveyor = null;

    public void Init(int _systemID, string _server, string _database, string _login, string _password)
    {
      ci.Server = _server;
      ci.Name = _database;
      ci.Login = _login;
      ci.Password = _password;
      ci.UsesWindowsAuthentication = false;
      api = ApiService.StartupNewApplicationWithConnectionSettings(ci);
      systemID = _systemID;
      InitConveyorParameters();
      Conveyor = new ConveyorProceesLogic(systemID, ci.Server, ci.Name, ci.Login, ci.Password, true);
    }

    public DbSystem system = new DbSystem();
    public int communicationsControllerSystemID;
    public DbSystem communicationControllerSystem;
    public DbSystem nextConveyorSystem;
    public DbSystem altConveyorSystem;
    public DbSystem ejectConveyorSystem;
    public string lineReference;
    public string plc_PositionID;
    public string nextConv_PositionID;
    public string altConv_PositionID;
    public string ejectConv_PositionID;
    public int foSystemID;
    public int nextLinkedSystemID;
    public int ejectDestinationSystemID;
    public int alternateDestinationSystemID;
    public int altRoutingSystemID;
    public int routingAreaID;
    public int defaultRoute;
    public int warehouseLocationID;
    public string ConveyorReference;
    public DbSystem linkedRFIDReaderSystem;
    public int linkedRFIDReaderSystemID;
    public int ProcessErrorEventDefinitionID;
    public int RFIDTimeoutEventDefinitionID;
    public int RFIDNoReadEventDefinitionID;
    public int ManualRFIDUserTaskDefinitionID;
    public int ManualUserTaskDefinitionID;
    public int rfidReaderTimeout;
    public int palletItemDefinitionID;
    public int productItemDefinitionID;

    public enum TaskUserState
    {
      NEW, //0
      WIP, //1
      COMPLETE, //2
      FINALIZED, //3
      RETRY, //4
      CANCELLED = 11 //11
    }

    public enum TaskPassFail
    {
      FAIL, //0
      PASS, //1
      UNKNOWN = -1, //-1
    }

    public enum AtPositionState
    {
      STARTOFTRACKING, //0
      ENDOFTRACKING, //1
      ATDESTINATION, //2
      ATPOSITION, //3
      ERRORNOTSEEN //4
    }

    public enum ItemUserState
    {
      CREATED = 1,
      PRINTED = 2,
      SENTTOKAFKA = 4,
      REPRINTED = 8,
      RELEASETOSTORAGE = 16,
      ATFINALPOSITION = 32,
      PICKEDUP = 64,
      ERPCONFIRMED = 128,
      DISSOLVED = 256,
      DISSOLVEDCONFIRMED = 512,
      CANCELLED = 1024,
      PALLETDATAACKNOWLEDGED = 2048,
      UNDISSOLVE = 4096,
      PARTIALLYCONSUMED = 8192,
      FULLYCONSUMED = 16384
    }

    public enum GroupingResultState
    {
      ACCEPTED, //0
      COMPLETED, //1
      FAILED //2
    }

    public enum PalletConsumptionType
    {
      FULL = 1,
      PARTIAL = 2
    }

    //Test Methods

    //Checks
    public bool CheckTblProductRoutingAllocatedSystem(int ProductID, int AssignedSystem)
    {
      string sqlCheckAllocatedSystem = @"SELECT ConveyorSystemID 
                                         FROM tblProductRouting 
                                         WHERE ProductID = {0} 
                                         AND Allocated = 1".FormatWith(ProductID);
      int foundAllocatedSystemID = ExecSQLInt(sqlCheckAllocatedSystem);
      if (foundAllocatedSystemID != 0)
      {
        Assert.AreEqual(AssignedSystem, foundAllocatedSystemID);
      }
      return true;
    }

    public bool CheckTblProductRoutingAllocatedSystems(int ProductID, List<int> AssignedSystems)
    {
      string sqlCheckAllocatedSystem = @"SELECT ConveyorSystemID 
                                         FROM tblProductRouting 
                                         WHERE ProductID = {0} 
                                         AND Allocated = 1".FormatWith(ProductID);
      List<int> foundAllocatedSystemIDs = api.Util.Db.ExecuteScalar<List<int>>(sqlCheckAllocatedSystem).Return;
      for (int i = 0; i < foundAllocatedSystemIDs.Count; i++)
      {
        Assert.AreEqual(AssignedSystems[i], foundAllocatedSystemIDs[i]);
      }
      return true;
    }

    public int CheckTblProductRouting(int SystemID = 0, int ProductID = 0, bool checkLastProduct = true, bool checkLastPickup = true)
    {
      int amount;
      string sqlGetRowCountTblProductRouting = "SELECT COUNT (*) FROM tblProductRouting";
      amount = ExecSQLInt(sqlGetRowCountTblProductRouting);
      if (ProductID != 0 && SystemID != 0)
      {
        string CheckTblProductRoutingValues = @"SELECT  
                                                    CONVERT(DateTime,LastProduceTimeStamp) AS LastProduceTimeStamp,   
                                                    CONVERT(DateTime,LastPickUpTimeStamp) AS LastPickUpTimeStamp   
                                                FROM tblProductRouting
                                                WHERE ProductID = {0}
                                                AND ConveyorSystemID = {1}".FormatWith(ProductID, SystemID);
        DataTable dt = api.Util.Db.GetDataTable(CheckTblProductRoutingValues).Return;
        if (dt is null || dt.Rows.Count == 0)
        {
          Assert.Fail("TblProductRouting failed to retrieve");
        }
        foreach (DataRow dr in dt.Rows)
        {
          DateTime minDate = DateTime.Now.AddMinutes(-30);
          string LastProduceString = dr["LastProduceTimeStamp"].ToString();
          string LastPickupString = dr["LastPickUpTimeStamp"].ToString();
          if (LastProduceString != "")
          {
            if (checkLastProduct)
            {
              DateTime LastProduce = DateTime.Parse(dr["LastProduceTimeStamp"].ToString());
              if (minDate > LastProduce)
              {
                Assert.Fail("TblProductRouting LastProduce {0}".FormatWith(LastProduce));
              }
            }
          }
          if (LastPickupString != "")
          {
            if (checkLastPickup)
            {
              DateTime LastPickup = DateTime.Parse(dr["LastPickUpTimeStamp"].ToString());
              if (minDate > LastPickup)
              {
                Assert.Fail("TblProductRouting LastPickup {0}".FormatWith(LastPickup));
              }
            }
          }

        }
      }
      return amount;
    }

    public bool GetTaskStateComplete(int AtPositionTaskID)
    {
      string sql_GetTaskState = @"IF(SELECT CompletedDateTime FROM tblTaskPLC2WCS
                                  WHERE ID = {0}) IS NULL  SELECT  0 
                                  ELSE SELECT 1".FormatWith(AtPositionTaskID);
      bool completed = api.Util.Db.ExecuteScalar<bool>(sql_GetTaskState).Return;
      return completed;
    }

    public bool CheckEventCreatedByDefinitionID(int EventDefinitionID)
    {
      string sql_CheckEventCreated = @"SELECT TOP 1 * FROM tEvent 
                                       WHERE EventDefinitionID = {0}".FormatWith(EventDefinitionID);
      DbEvent Event = api.Data.DbEvent.Load.WithSql(sql_CheckEventCreated);
      Assert.IsNotNull(Event);

      return true;
    }

    public bool CheckProductData(int ID, string SSCC = "", int ItemDefinitionID = -1, int ParentItemID = -1, int LocationID = -2, string PLCReference = "-1", int FinalDestination = -1, int Quantity = -1)
    {
      try
      { // retrieve Item
        string sql_GetItem = "SELECT * FROM tItem WHERE ID = {0}".FormatWith(ID);
        DbItem retrievedItem = api.Data.DbItem.Load.WithSql(sql_GetItem);
        if (retrievedItem != null)
        {
          // Check all attributes
          if (SSCC != "")
            Assert.AreEqual(SSCC, retrievedItem.UniqueID);
          if (ItemDefinitionID != -1)
            Assert.AreEqual(ItemDefinitionID, retrievedItem.ItemDefinitionID);
          if (ParentItemID != -1)
            Assert.AreEqual(ParentItemID, retrievedItem.ParentItemID);
          if (LocationID != -2)
            Assert.AreEqual(LocationID, retrievedItem.LocationID);
          if (PLCReference != "-1")
            Assert.AreEqual(PLCReference, retrievedItem.Attribute01);
          if (FinalDestination != -1)
            Assert.AreEqual(FinalDestination, retrievedItem.Attribute16.AsInt(-1));
          if (Quantity != -1)
            Assert.AreEqual(Quantity, retrievedItem.Quantity);
        }
        else
        {
          Assert.Fail("ProductItem Could not be found");
          return false;
        }
        return true;
      }
      catch
      {
        Assert.Fail("Product check failed");
        return false;
      }
    }

    public bool CheckPalletData(int ID, string GRAI = "", int ItemDefinitionID = -1, int LocationID = -2, string PLCReference = "-1", int FinalDestination = -1, int parentItemID = 0)
    {
      try
      {
        // retrieve Pallet
        DbItem retrievedItem = api.Data.DbItem.Load.ByID(ID);
        if (retrievedItem != null)
        {
          // Check all attributes
          if (GRAI != "")
            Assert.AreEqual(GRAI, retrievedItem.UniqueID);
          if (ItemDefinitionID != -1)
            Assert.AreEqual(ItemDefinitionID, retrievedItem.ItemDefinitionID);
          if (LocationID != -2)
            Assert.AreEqual(LocationID, retrievedItem.LocationID);
          if (!PLCReference.Equals("-1"))
            Assert.AreEqual(PLCReference, retrievedItem.Attribute01);
          if (FinalDestination != -1)
            Assert.AreEqual(FinalDestination, retrievedItem.Attribute16.AsInt(-1));
          if (parentItemID != 0)
            Assert.AreEqual(parentItemID, retrievedItem.ParentItemID);
        }
        else
        {
          Assert.Fail("PalletItem Could not be found");
          return false;
        }
        return true;
      }
      catch
      {
        Assert.Fail("CheckPalletData() failed");
        return false;
      }
    }

    public int CheckSetDestinationExist()
    {
      int result = -1;
      string sql_CheckSetDest = @"SELECT TOP 1 * FROM tblTaskWCS2PLC 
                                  WHERE TelegramType = 110 
                                  AND PositionID = {0}
                                  ORDER BY ID DESC".FormatWith(plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckSetDest);
      if (!(dt is null || dt.Rows.Count == 0))
      {
        result = dt.Rows[0].GetInteger("ID", -1);
      }
      return result;
    }

    public bool CheckSetDestination(string ExpectedToPostionID)
    {
      string sql_CheckSetDest = @"SELECT TOP 1 * FROM tblTaskWCS2PLC 
                                  WHERE TelegramType = 110 
                                  AND PositionID = {0}
                                  ORDER BY ID DESC".FormatWith(plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckSetDest);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("SetDestination does not exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedToPostionID, dr["ToPositionID"].ToString());
      }
      return true;
    }

    public bool CheckPalletAssignedToOrder(string ExpectedSSCC, string ExpectedFO)
    {
      DbSystem kafkaSystem = api.Data.DbSystem.Load.ByKeyAndSiteID("DS.KAFKAINTERFACE", 1);
      Assert.IsNotNull(kafkaSystem);
      DbTaskDefinition taskDefinition = api.Data.DbTaskDefinition.Load.ByKeyAndSystemID("WCS2WMS.PALLETASSTOORDER", kafkaSystem.ID);
      Assert.IsNotNull(taskDefinition);
      DbTaskVtrDefinition taskVtrDefPallet = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.PALLETID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefOrder = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.ORDERID", taskDefinition.ID);
      Assert.IsNotNull(taskVtrDefPallet);
      Assert.IsNotNull(taskVtrDefOrder);
      string sql_CheckPalletAssignedToOrder = @"SELECT TOP 1 * FROM tTask 
                                                WHERE TaskDefinitionID = {0}
                                                ORDER BY ID DESC".FormatWith(taskDefinition.ID);
      DbTask palletAssignedToOrderTask = api.Data.DbTask.Load.WithSql(sql_CheckPalletAssignedToOrder);
      Assert.IsNotNull(palletAssignedToOrderTask);

      string sql_CheckVtrValues = @"SELECT TOP 1 TagValue FROM tTaskVtr 
                                    WHERE TaskID = {0}
                                    AND TaskVtrDefinitionID = {1}
                                    ORDER BY ID DESC";
      string sscc = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefPallet.ID));
      string foNumber = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefOrder.ID));
      Assert.AreEqual(ExpectedSSCC, sscc);
      Assert.AreEqual(ExpectedFO, foNumber);      
      return true;
    }

    public bool CheckPalletAssignedToOrder(string ExpectedSSCC, string ExpectedFO, int ExpectedQty, PalletConsumptionType ExpectedConsumptionType)
    {
      DbSystem kafkaSystem = api.Data.DbSystem.Load.ByKeyAndSiteID("DS.KAFKAINTERFACE", 1);
      Assert.IsNotNull(kafkaSystem);
      DbTaskDefinition taskDefinition = api.Data.DbTaskDefinition.Load.ByKeyAndSystemID("WCS2WMS.PALLETASSTOORDER", kafkaSystem.ID);
      Assert.IsNotNull(taskDefinition);
      DbTaskVtrDefinition taskVtrDefPallet = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.PALLETID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefOrder = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.ORDERID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefItemsConsumed = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.ITEMSCONSUMED", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefFullOrPartial = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.FULLORPARTIAL", taskDefinition.ID);
      Assert.IsNotNull(taskVtrDefPallet);
      Assert.IsNotNull(taskVtrDefOrder);
      Assert.IsNotNull(taskVtrDefItemsConsumed);
      Assert.IsNotNull(taskVtrDefFullOrPartial);
      string sql_CheckPalletAssignedToOrder = @"SELECT TOP 1 * FROM tTask 
                                                WHERE TaskDefinitionID = {0}
                                                ORDER BY ID DESC".FormatWith(taskDefinition.ID);
      DbTask palletAssignedToOrderTask = api.Data.DbTask.Load.WithSql(sql_CheckPalletAssignedToOrder);
      Assert.IsNotNull(palletAssignedToOrderTask);

      string sql_CheckVtrValues = @"SELECT TOP 1 TagValue FROM tTaskVtr 
                                    WHERE TaskID = {0}
                                    AND TaskVtrDefinitionID = {1}
                                    ORDER BY ID DESC";
      string sscc = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefPallet.ID));
      string foNumber = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefOrder.ID));
      int itemsConsumed = ExecSQLInt(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefItemsConsumed.ID));
      int fullOrPartial = ExecSQLInt(sql_CheckVtrValues.FormatWith(palletAssignedToOrderTask.ID, taskVtrDefFullOrPartial.ID));
      Assert.AreEqual(ExpectedSSCC, sscc);
      Assert.AreEqual(ExpectedFO, foNumber);
      Assert.AreEqual(ExpectedQty, itemsConsumed);
      Assert.AreEqual((int)ExpectedConsumptionType, fullOrPartial);
      return true;
    }

    public bool CheckPalletAtLocation(string ExpectedSSCC, string ExpectedLocID, string ExpectedLocArea)
    {
      DbSystem kafkaSystem = api.Data.DbSystem.Load.ByKeyAndSiteID("DS.KAFKAINTERFACE", 1);
      Assert.IsNotNull(kafkaSystem);
      DbTaskDefinition taskDefinition = api.Data.DbTaskDefinition.Load.ByKeyAndSystemID("WCS2WMS.PALLETATLOCATION", kafkaSystem.ID);
      Assert.IsNotNull(taskDefinition);
      DbTaskVtrDefinition taskVtrDefPallet = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.PALLETID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefLocID = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.LOCATIONID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefLocArea = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.LOCATIONAREA", taskDefinition.ID);
      Assert.IsNotNull(taskVtrDefPallet);
      Assert.IsNotNull(taskVtrDefLocID);
      Assert.IsNotNull(taskVtrDefLocArea);
      string sql_CheckPalletAtLocation = @"SELECT TOP 1 * FROM tTask 
                                           WHERE TaskDefinitionID = {0}
                                           ORDER BY ID DESC".FormatWith(taskDefinition.ID);
      DbTask palletAtLocationTask = api.Data.DbTask.Load.WithSql(sql_CheckPalletAtLocation);
      Assert.IsNotNull(palletAtLocationTask);

      string sql_CheckVtrValues = @"SELECT TOP 1 TagValue FROM tTaskVtr 
                                    WHERE TaskID = {0}
                                    AND TaskVtrDefinitionID = {1}
                                    ORDER BY ID DESC";
      string sscc = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAtLocationTask.ID, taskVtrDefPallet.ID));
      string locID = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAtLocationTask.ID, taskVtrDefLocID.ID));
      string locArea = ExecSQLString(sql_CheckVtrValues.FormatWith(palletAtLocationTask.ID, taskVtrDefLocArea.ID));
      Assert.AreEqual(ExpectedSSCC, sscc);
      Assert.AreEqual(ExpectedLocID, locID);
      Assert.AreEqual(ExpectedLocArea, locArea);

      return true;
    }

    public bool CheckPalletOperationRequest(string ExpectedSSCC, string ExpectedSourceLocArea,
      string ExpectedSourceLocID, string ExpectedDestLocArea, string ExpectedDestLocID, int ExpectedPriority)
    {
      DbSystem kafkaSystem = api.Data.DbSystem.Load.ByKeyAndSiteID("DS.KAFKAINTERFACE", 1);
      Assert.IsNotNull(kafkaSystem);
      DbTaskDefinition taskDefinition = api.Data.DbTaskDefinition.Load.ByKeyAndSystemID("WCS2MTTC.PALLETOPERATIONREQ", kafkaSystem.ID);
      Assert.IsNotNull(taskDefinition);
      DbTaskVtrDefinition taskVtrDefPalletIDs = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.PALLETIDS", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefSourceLocArea = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.SOURCELOCATIONAREA", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefSourceLocID = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.SOURCELOCATIONID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefDestLocArea = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.DESTINATIONLOCATIONAREA", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefDestLocID = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.DESTINATIONLOCATIONID", taskDefinition.ID);
      DbTaskVtrDefinition taskVtrDefPriority = api.Data.DbTaskVtrDefinition.Load.ByKeyAndTaskDefinitionID("TVTR.PRIORITY", taskDefinition.ID);
      Assert.IsNotNull(taskVtrDefPalletIDs);
      Assert.IsNotNull(taskVtrDefSourceLocArea);
      Assert.IsNotNull(taskVtrDefSourceLocID);
      Assert.IsNotNull(taskVtrDefDestLocArea);
      Assert.IsNotNull(taskVtrDefDestLocID);
      Assert.IsNotNull(taskVtrDefPriority);
      string sql_CheckPalletOperationReq = @"SELECT TOP 1 * FROM tTask 
                                           WHERE TaskDefinitionID = {0}
                                           ORDER BY ID DESC".FormatWith(taskDefinition.ID);
      DbTask palletOperationReqTask = api.Data.DbTask.Load.WithSql(sql_CheckPalletOperationReq);
      Assert.IsNotNull(palletOperationReqTask);

      string sql_CheckVtrValues = @"SELECT TOP 1 TagValue FROM tTaskVtr 
                                    WHERE TaskID = {0}
                                    AND TaskVtrDefinitionID = {1}
                                    ORDER BY ID DESC";
      string sscc = ExecSQLString(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefPalletIDs.ID));
      string sourceLocArea = ExecSQLString(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefSourceLocArea.ID));
      string sourceLocID = ExecSQLString(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefSourceLocID.ID));
      string destLocArea = ExecSQLString(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefDestLocArea.ID));
      string destLocID = ExecSQLString(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefDestLocID.ID));
      int priority = ExecSQLInt(sql_CheckVtrValues.FormatWith(palletOperationReqTask.ID, taskVtrDefPriority.ID));
      Assert.AreEqual(ExpectedSSCC, sscc);
      Assert.AreEqual(ExpectedSourceLocArea, sourceLocArea);
      Assert.AreEqual(ExpectedSourceLocID, sourceLocID);
      Assert.AreEqual(ExpectedDestLocArea, destLocArea);
      Assert.AreEqual(ExpectedDestLocID, destLocID);
      Assert.AreEqual(ExpectedPriority, priority);

      return true;
    }

    public bool CheckSetDestinationItemID(string ExpectedItemID)
    {
      string sql_CheckSetDest = @"SELECT TOP 1 * FROM tblTaskWCS2PLC 
                                  WHERE TelegramType = 110 
                                  AND PositionID = {0}
                                  ORDER BY ID DESC".FormatWith(plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckSetDest);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("SetDestination does not exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedItemID, dr["ItemID"].ToString());
      }
      return true;
    }

    public bool CheckMessagePassFail(int TelegramType, TaskPassFail ExpectedPassFail)
    {
      string sql_CheckMsgPassFail = @"SELECT top 1 * FROM tblTaskPLC2WCS 
                                  WHERE TelegramType = {0}
                                  AND (PositionID = {1} OR GroupPositionID = {1})".FormatWith(TelegramType, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckMsgPassFail);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail(" Message does not Exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedPassFail, (TaskPassFail)dr["PassFail"]);
      }
      return true;
    }

    public bool CheckMessageUserState(int TelegramType, TaskUserState ExpectedUserState)
    {
      string sql_CheckMsgUserState = @"SELECT top 1 * FROM tblTaskPLC2WCS 
                                       WHERE TelegramType = {0}
                                       AND (PositionID = {1} OR GroupPositionID = {1})".FormatWith(TelegramType, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckMsgUserState);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("Message does not exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedUserState, (TaskUserState)dr["UserState"]);
      }
      return true;
    }

    public bool CheckMessagePassFailByMessageID(int MessageID, TaskPassFail ExpectedPassFail)
    {
      string sql_CheckMsgPassFail = @"SELECT TOP 1 * FROM tblTaskPLC2WCS WHERE ID = {0}".FormatWith(MessageID);
      DataTable dt = GetDataTable(sql_CheckMsgPassFail);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail(" Message does not exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedPassFail, (TaskPassFail)dr["PassFail"]);
      }
      return true;
    }

    public bool CheckMessageUserStateByMessageID(int MessageID, TaskUserState ExpectedUserState)
    {
      string sql_CheckMsgUserState = @"SELECT top 1 * FROM tblTaskPLC2WCS WHERE ID = {0}".FormatWith(MessageID, plc_PositionID);
      DataTable dt = GetDataTable(sql_CheckMsgUserState);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("Message does not exist");
        return false;
      }
      foreach (DataRow dr in dt.Rows)
      {
        Assert.AreEqual(ExpectedUserState, (TaskUserState)dr["UserState"]);
      }
      return true;
    }

    public void CheckGrouped(int ParentPalletItemID, int ChildPalletItemID, bool NotGrouped = false)
    {
      string sql_CheckGrouped = @"SELECT ID FROM tItemSourceItem 
                                  WHERE ItemID = {0}
                                  AND SourceItemID = {1}".FormatWith(ParentPalletItemID, ChildPalletItemID);
      int ID = ExecSQLInt(sql_CheckGrouped);

      if (NotGrouped && ID > 0)
      {
        Assert.Fail("Grouping Item found");
      }
      if (!NotGrouped && ID == 0)
      {
        Assert.Fail("No Grouping Item found");
      }

    }

    public bool CheckProductParentItemID(int ProductID, int ExpectedParentItemID)
    {
      string sql_CheckProductParentItemID = @"SELECT TOP 1 * FROM tItem
                                              WHERE ID = {0}
                                              AND ItemDefinitionID = 11".FormatWith(ProductID);
      DbItem ProductItem = api.Data.DbItem.Load.WithSql(sql_CheckProductParentItemID);
      Assert.IsNotNull(ProductItem);
      Assert.AreEqual(ExpectedParentItemID, ProductItem.ParentItemID);

      return true;
    }

    public void CheckProductQuantity(int ProductID, int ExpectedQuantity)
    {
      DbItem productItem = api.Data.DbItem.Load.ByID(ProductID);
      Assert.IsNotNull(productItem);
      Assert.AreEqual(ExpectedQuantity, productItem.Quantity);
    }

    public void CheckItemUserState(int ItemID, ItemUserState ExpectedUserState, bool ContainsUserState = true)
    {
      DbItem productItem = api.Data.DbItem.Load.ByID(ItemID);
      Assert.IsNotNull(productItem);
      if (ContainsUserState)
      {
        if ((productItem.UserState & (int)ExpectedUserState) == 0) Assert.Fail();
      }
      else
      {
        if ((productItem.UserState & (int)ExpectedUserState) > 0) Assert.Fail();
      }
    }

    public bool CheckInitialQuantity(int ProductID, int ExpectedQuantity)
    {
      string sql_CheckInitialQuantity = @"SELECT TOP 1 * FROM tItem
                                                WHERE ID = {0}
                                                AND ItemDefinitionID = 11".FormatWith(ProductID);
      DbItem productItem = api.Data.DbItem.Load.WithSql(sql_CheckInitialQuantity);
      if (productItem == null)
      {
        Assert.Fail("Product does not exist");
        return false;
      }
      Assert.AreEqual(ExpectedQuantity.ToString(), productItem.Attribute05);
      return true;
    }

    public bool CheckUserTaskUserState(int TaskID, TaskUserState ExpectedUserState)
    {
      DbTask UserTask = api.Data.DbTask.Load.ByID(TaskID);
      if (UserTask == null)
      {
        Assert.Fail("UserTask does not exist");
        return false;
      }
      Assert.AreEqual(ExpectedUserState, (TaskUserState)UserTask.UserState);
      return true;
    }

    public int CheckUserTask(int TaskDefinitionID, int PLCMessageTaskID)
    {
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      string sql_CheckPLCMessageUserTask = @"SELECT TOP 1 UserTaskID 
                                             FROM tblTaskPLC2WCS 
                                             WHERE ID = {0}".FormatWith(PLCMessageTaskID);
      int UserTaskID = ExecSQLInt(sql_CheckPLCMessageUserTask);
      Assert.AreEqual(UserTask.ID, UserTaskID);
      return UserTask.ID;
    }

    public bool CheckUserTaskPassFail(int TaskID, TaskPassFail ExpectedPassFail)
    {
      DbTask UserTask = api.Data.DbTask.Load.ByID(TaskID);
      if (UserTask == null)
      {
        Assert.Fail("UserTask does not exist");
        return false;
      }
      Assert.AreEqual(ExpectedPassFail, (TaskPassFail)UserTask.PassFail);
      return true;
    }

    public void CheckRFIDTags(string ExpectedTagValue)
    {
      Assert.IsNotNull(linkedRFIDReaderSystem);
      string tagName = "LastReadTag";
      for (int i = 0; i < 6; i++)
      {
        string t = "";
        if (i > 0) t = "_" + i.ToString();

        string tagValue = GetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + tagName + t);
        Assert.AreEqual(ExpectedTagValue, tagValue);
      }
    }

    public void CheckMaterialConsumption(DbItem ProductItem, int ExpectedQuantity)
    {
      string sql_GetProductMaterial = @"SELECT TOP 1 * FROM tProductMaterial
                                        WHERE ProductID = {0} 
                                        ORDER BY ID DESC".FormatWith(ProductItem.ProductID);
      DbProductMaterial productMaterial = api.Data.DbProductMaterial.Load.WithSql(sql_GetProductMaterial);
      Assert.IsNotNull(productMaterial);
      string sql_CheckMaterialConsumption = @"SELECT TOP 1 * FROM tMaterialUseActual 
                                              WHERE SystemID = {0} 
                                              AND JobID = {1} 
                                              AND MaterialID = {2}
                                              ORDER BY ID DESC".FormatWith(foSystemID, ProductItem.JobID, productMaterial.MaterialID);
      DbMaterialUseActual mua = api.Data.DbMaterialUseActual.Load.WithSql(sql_CheckMaterialConsumption);
      Assert.IsNotNull(mua);
      Assert.AreEqual(ExpectedQuantity, mua.Quantity);
    }

    public void CheckPalletIsRemoved(int PalletItemID)
    {
      string sql_CheckPalletIsRemoved = @"SELECT TOP 1 * FROM tItem
                                          WHERE ID = {0}
                                          AND ItemDefinitionID = 10".FormatWith(PalletItemID);
      DbItem PalletItem = api.Data.DbItem.Load.WithSql(sql_CheckPalletIsRemoved);
      Assert.IsNull(PalletItem);
    }

    //Helper Functions and DAL
    private void InitConveyorParameters()
    {
      system = api.Data.DbSystem.Load.ByID(systemID);
      Assert.IsNotNull(system);
      communicationsControllerSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONVGENERAL.CP.CONV.COMMSCONTROLLERID").AsInt(-1);
      communicationControllerSystem = api.Data.DbSystem.Load.ByID(communicationsControllerSystemID);
      lineReference = GetSystemCustomPropertyValue(communicationControllerSystem.ID, "CPS.PLCCONFIG.CP.LINEREF").AsString();
      plc_PositionID = GetSystemCustomPropertyValue(system.ID, "CPS.CONVGENERAL.CP.CONV.PLCPOSITIONID").AsString();
      nextLinkedSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONVGENERAL.CP.CONV.NEXTLINKEDCONVEYORSYSTEMID").AsInt(-1);
      nextConveyorSystem = api.Data.DbSystem.Load.ByID(nextLinkedSystemID);
      ejectDestinationSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONVGENERAL.CP.CONV.EJECTLINKEDCONVEYORSYSTEMID").AsInt(-1);
      ejectConveyorSystem = api.Data.DbSystem.Load.ByID(ejectDestinationSystemID);
      alternateDestinationSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONVGENERAL.CP.CONV.ALTLINKEDCONVEYORSYSTEMID").AsInt(-1);
      altConveyorSystem = api.Data.DbSystem.Load.ByID(alternateDestinationSystemID);
      altRoutingSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.ROUTING.CP.ALTROUTINGSYSTEMID").AsInt(-1);
      routingAreaID = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.ROUTING.CP.ROUTINGAREAID").AsInt(-1);
      defaultRoute = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.ROUTING.CP.CONV.DEFAULTROUTE").AsInt(-1);
      foSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.FOSYSTEM.CP.JOBSYSTEMID").AsInt(-1);
      linkedRFIDReaderSystemID = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERSYSTEMID").AsInt(-1);
      if (linkedRFIDReaderSystemID > 0)
      {
        linkedRFIDReaderSystem = api.Data.DbSystem.Load.ByID(linkedRFIDReaderSystemID).ThrowIfNull("RFIDReader system not found");
        rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
        RFIDTimeoutEventDefinitionID = GetEventDefinition("ED.RFID.TIMEOUT", system.ID).ID;
        RFIDNoReadEventDefinitionID = GetEventDefinition("ED.RFID.NOREAD", system.ID).ID;
        ManualRFIDUserTaskDefinitionID = GetTaskDefinition("TD.RFID.MANUALTASK", system.ID).ID;
      }
      ProcessErrorEventDefinitionID = GetEventDefinition("ED.PROCESSERROR", system.ID).ID;
      palletItemDefinitionID = api.Data.DbItemDefinition.Load.ByCode("ID.PALLET").ID;
      productItemDefinitionID = api.Data.DbItemDefinition.Load.ByCode("ID.PALLETIZEDPRODUCT").ID;
      DbTaskDefinition ManualUserTaskDefinition = GetTaskDefinition("TD.MANUALLYPROCESSPALLET", system.ID);
      if (ManualUserTaskDefinition != null) ManualUserTaskDefinitionID = ManualUserTaskDefinition.ID;

      if (nextLinkedSystemID != -1)
      {
        nextConv_PositionID = GetSystemCustomPropertyValue(nextLinkedSystemID, "CPS.CONVGENERAL.CP.CONV.PLCPOSITIONID").AsString();
      }
      if (alternateDestinationSystemID != -1)
      {
        altConv_PositionID = GetSystemCustomPropertyValue(alternateDestinationSystemID, "CPS.CONVGENERAL.CP.CONV.PLCPOSITIONID").AsString();
      }
      if (ejectDestinationSystemID != -1)
      {
        ejectConv_PositionID = GetSystemCustomPropertyValue(ejectDestinationSystemID, "CPS.CONVGENERAL.CP.CONV.PLCPOSITIONID").AsString();
      }
      warehouseLocationID = GetWarehouseLocationIDByKey("LOC.RW.%.WAREHOUSE");
      ConveyorReference = lineReference + "." + plc_PositionID.Substring(6);
    }

    public DbTaskDefinition GetTaskDefinition(string Key, int systemID)
    {
      DbTaskDefinition td = null;
      try
      {
        td = api.Data.DbTaskDefinition.Load.ByKeyAndSystemID(Key, systemID);
      }
      catch (Exception ex)
      {
        Assert.Fail(ex.Message, true);
      }
      return td;
    }

    public DbEventDefinition GetEventDefinition(string Key, int systemID)
    {
      DbEventDefinition ed = null;
      try
      {
        ed = api.Data.DbEventDefinition.Load.ByKeyAndSystemID(Key, systemID);
      }
      catch (Exception ex)
      {
        Assert.Fail("GetEventDefinition() ERROR: " + ex.Message);
      }
      return ed;
    }

    public void SetSystemCustomPropertyValue(int SystemID, string CPKey, int value)
    {
      try
      {
        string sqlUpdateRFIDTimeOut = @"UPDATE tSystemCustomProperty
                                        SET VALUE = {0}
                                        WHERE SystemID = {1} 
                                        AND CustomPropertyID = (SELECT ID FROM tCustomProperty 
                                                                WHERE [Key] LIKE '%{2}%')".FormatWith(value.ToSql(), SystemID, CPKey);
        ExecSQL(sqlUpdateRFIDTimeOut);
      }
      catch (Exception ex)
      {
        Assert.Fail("Failed to Set/update System custom property. " + ex.Message);
      }
    }

    public void SetSystemCustomPropertyValue(int SystemID, string CPKey, string value)
    {
      try
      {
        string sqlUpdateRFIDTimeOut = @"UPDATE tSystemCustomProperty
                                        SET VALUE = {0}
                                        WHERE SystemID = {1} 
                                        AND CustomPropertyID = (SELECT ID FROM tCustomProperty 
                                                                WHERE [Key] LIKE '%{2}%')".FormatWith(value.ToSql(), SystemID, CPKey);
        ExecSQL(sqlUpdateRFIDTimeOut);
      }
      catch (Exception ex)
      {
        Assert.Fail("Failed to Set/update System custom property. " + ex.Message);
      }
    }

    public void SetProductCustomPropertyValue(int value, int ProductID, string CPKey)
    {
      try
      {
        string sqlUpdateCustomProperty = @"UPDATE tProductCustomProperty 
                                                   SET VALUE = {0}
                                                   WHERE ProductID = {1}
                                                   AND CustomPropertyID = 
                                                            (SELECT ID FROM tCustomProperty 
                                                             WHERE [key] LIKE '%{2}%' )".FormatWith(value, ProductID, CPKey);
        ExecSQL(sqlUpdateCustomProperty);
      }
      catch (Exception ex)
      {
        Assert.Fail("Failed to Set/update Product custom property. " + ex.Message);
      }
    }

    public string GetSystemCustomPropertyValue(int systemID, string key)
    {
      string value = "";
      try
      {
        DbSystem system = api.Data.DbSystem.Load.ByID(systemID);
        if (system == null) Assert.Fail("System not found for custom property retrieval.");
        value = system.CustomProperties[key].Value;
      }
      catch (Exception ex)
      {
        Assert.Fail("Failed to retrieve system custom property. " + ex.Message);
      }
      return value;
    }

    public void InsertMaterialTestDataIntoTblProductRouting(int ProductID, int AltRoutingDestinationID = 180, bool assignAlt = false)
    {
      try
      {
        string insertTblProductRoutingProductProductID = @"
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 0, {1}, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 0, 167, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 0, 168, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 1, 169, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 1, 170, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 0, 171, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ( [ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES ( {0}, 0, 172, CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T12:06:08.0000000+01:00' AS DateTimeOffset), 1)
                            ".FormatWith(ProductID, AltRoutingDestinationID);
        ExecSQL(insertTblProductRoutingProductProductID);
        if (assignAlt)
        {
          string sqlUnassign = @"UPDATE tblProductRouting 
                                           SET Allocated = 0
                                           WHERE ProductID = {0}".FormatWith(ProductID);
          string sqlAssignAlt = @"UPDATE tblProductRouting 
                                           SET Allocated = 1
                                           WHERE ProductID = {0}
                                           AND ConveyorSystemID = {1}".FormatWith(ProductID, AltRoutingDestinationID);
          ExecSQL(sqlUnassign);
          ExecSQL(sqlAssignAlt);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("InsertMaterialTestDataIntoTblProductRouting()" + ex.Message);
        Assert.Fail("Failed to InsertMaterialTestDataIntoTblProductRouting");
      }
    }

    public void InsertTestDataIntoTblProductRouting(int AltRoutingDestinationID = 180)
    {
      try
      {
        string sqlInsertTestData = @"
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, {0}, CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, 167, CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, 168, CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, 169, CAST(N'2023-01-16T11:54:03.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, 170, CAST(N'2023-01-16T11:54:03.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 0, 171, CAST(N'2023-01-16T11:54:03.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (201, 1, 172, CAST(N'2023-01-16T11:54:03.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 1, {0}, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 167, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 168, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 169, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 170, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 171, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (50, 0, 172, CAST(N'2023-01-16T11:56:50.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, {0}, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, 167, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, 168, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, 169, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, 170, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 0, 171, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            INSERT [dbo].[tblProductRouting] ([ProductID], [Allocated], [ConveyorSystemID], [LastProduceTimeStamp], [LastPickUpTimeStamp], [GroupingEnabled]) VALUES (380, 1, 172, CAST(N'2023-01-16T12:01:11.0000000+01:00' AS DateTimeOffset), CAST(N'2023-01-16T11:54:02.0000000+01:00' AS DateTimeOffset), 1)
                            ".FormatWith(AltRoutingDestinationID);
        ExecSQL(sqlInsertTestData);
      }
      catch (Exception ex)
      {
        Console.WriteLine("InsertTestDataIntoTblProductRouting()" + ex.Message);
        Assert.Fail("tblProductRouting Failed to Insert Test Data");
      }
    }

    public void InsertTestDataIntoTblProductRoutingSetup(int JobID)
    {
      try
      {
        DbJob job = api.Data.DbJob.Load.ByID(JobID);
        Assert.IsNotNull(job);

        string sqlInsertTestData = @"
         INSERT INTO tblProductRoutingSetup 
          ([ProductID], [ConveyorSystemID], [Allocated], [UpdateDateTime], [UserName], [Note])
         VALUES
	      ({0}, 167, 0, SYSDATETIMEOFFSET(), '', ''), 
	      ({0}, 168, 0, SYSDATETIMEOFFSET(), '', ''), 
	      ({0}, 169, 1, SYSDATETIMEOFFSET(), '', ''), 
	      ({0}, 170, 1, SYSDATETIMEOFFSET(), '', ''), 
	      ({0}, 171, 0, SYSDATETIMEOFFSET(), '', ''), 
	      ({0}, 172, 0, SYSDATETIMEOFFSET(), '', '')
        ".FormatWith(job.ProductID);
        ExecSQL(sqlInsertTestData);
      }
      catch (Exception ex)
      {
        Console.WriteLine("InsertTestDataIntoTblProductRoutingSetup()" + ex.Message);
        Assert.Fail("tblProductRoutingSetup Failed to Insert Test Data");
      }
    }

    public void GroupPallets(DbItem ParentItem, DbItem ChildPallet)
    {
      try
      {
        string sql = @"DECLARE @ItemSourceItemID int 
                               EXEC spCore_Sequence_NextID_Out 'ItemSourceItemID', @ItemSourceItemID OUT 
                               INSERT INTO tItemSourceItem
                                ([ID], [SourceItemID], [ItemID], [ModifiedDateTime], [UploadedDateTime])
                                VALUES
                               (@ItemSourceItemID, {0}, {1}, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET())".FormatWith(ChildPallet.ID, ParentItem.ID);
        ExecSQL(sql);
      }
      catch (Exception ex)
      {
        Console.WriteLine("GroupPallets()" + ex.Message);
        Assert.Fail("GroupPallets Failed to group pallets");
      }
    }

    public void RemovePalletsFromConveyor(int ConveyorSystemID)
    {
      DbSystem conveyorSystem = api.Data.DbSystem.Load.ByID(ConveyorSystemID);
      Assert.IsNotNull(conveyorSystem);

      string sql_RemovePallets = @"UPDATE tItem 
                                   SET LocationID = {0}
                                   WHERE LocationID = {1}".FormatWith(warehouseLocationID, conveyorSystem.LocationID);
      ExecSQL(sql_RemovePallets);
    }

    public DbSystem GetPreviousConveyorSystem(int CurrentConveyorSystemID)
    {
      DbSystem PreviousConveyorSystem;
      string sql;
      try
      {
        sql = @"SELECT TOP 1 s.* FROM tSystem s 
                INNER JOIN viewCustomPropertySystem v ON v.ID = s.ID 
                WHERE v.[CPS.CONVGENERAL.CP.CONV.NEXTLINKEDCONVEYORSYSTEMID] = {0}".FormatWith(CurrentConveyorSystemID);
        PreviousConveyorSystem = api.Data.DbSystem.Load.WithSql(sql);
      }
      catch (Exception ex)
      {
        Assert.Fail("Previous conveyor not found. " + ex.Message);
        return null;
      }
      return PreviousConveyorSystem;
    }

    public DbSystem GetPreviousDiverterConveyorSystem(int CurrentConveyorSystemID)
    {
      DbSystem PreviousDiverterConveyorSystem;
      string sql;
      try
      {
        sql = @"SELECT TOP 1 s.* FROM tSystem s 
                INNER JOIN viewCustomPropertySystem v ON v.ID = s.ID 
                WHERE v.[CPS.CONVGENERAL.CP.CONV.EJECTLINKEDCONVEYORSYSTEMID] = {0}".FormatWith(CurrentConveyorSystemID);
        PreviousDiverterConveyorSystem = api.Data.DbSystem.Load.WithSql(sql);
      }
      catch (Exception ex)
      {
        Assert.Fail("Previous diverter conveyor not found. " + ex.Message);
        return null;
      }
      return PreviousDiverterConveyorSystem;
    }

    public int GetWarehouseLocationIDByKey(string WarehouseKey)
    {
      int LocationID = -1;
      try
      {
        string sql = "SELECT * FROM tLocation WHERE UniqueID LIKE {0}".FormatWith(WarehouseKey.ToSql());
        LocationID = api.Data.DbLocation.Load.WithSql(sql).ID;
      }
      catch (Exception ex)
      {
        Assert.Fail("Warehouse Location ID not found. " + ex.Message);
      }
      return LocationID;
    }

    public void ExecSQL(string sql)
    {
      api.Util.Db.ExecuteSql(sql);
    }

    public int ExecSQLInt(string sql)
    {
      int result = api.Util.Db.ExecuteScalar<int>(sql).Return;
      return result;
    }

    public string ExecSQLString(string sql)
    {
      string result = api.Util.Db.ExecuteScalar<string>(sql).Return;
      return result;
    }

    public DataTable GetDataTable(string sql)
    {
      return api.Util.Db.GetDataTable(sql).Return;
    }

    public int SendAtPosition(int PLC_ItemID, int state)
    {
      string sql_AtPosition = "EXEC usp_P2W_CreateAtPosition {0}, {1}, {2}, {3}".FormatWith(communicationControllerSystem.ID, PLC_ItemID, plc_PositionID.ToSql(), state);
      ExecSQL(sql_AtPosition);
      string sql_GetAtPositionTask = @"SELECT TOP 1 * FROM tblTaskPLC2WCS 
                                             WHERE TelegramType = {0}
                                             AND PositionID = {1}
                                             ORDER BY ID DESC".FormatWith(120, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_GetAtPositionTask);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("SendAtPosition could not be created");
        return -1;
      }
      return dt.Rows[0].GetInteger("ID", -1);
    }

    public int SendGroupingResult(int ParentPLCItemID, int Status)
    {
      string sql_SendGroupingResult = "EXEC usp_P2W_CreateGroupingResult {0}, {1}, {2}, {3}, {4}".FormatWith(communicationControllerSystem.ID, plc_PositionID.ToSql(), ParentPLCItemID, Status, string.Empty.ToSql());
      ExecSQL(sql_SendGroupingResult);
      string sql_CheckExist = @"SELECT TOP 1 * FROM tblTaskPLC2WCS 
                                WHERE TelegramType = {0}
                                AND GroupPositionID = {1}
                                ORDER BY ID DESC".FormatWith(211, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckExist);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("No Grouping Result Task Found");
      }
      return dt.Rows[0].GetInteger("ID", -1);
    }

    public int SendStackingResult(int parentItemID, int childItemID, int state, string description = "")
    {
      string sql = "EXEC usp_P2W_CreateStackingResult {0}, {1}, {2}, {3}, {4}, {5}".FormatWith(communicationControllerSystem.ID, plc_PositionID.ToSql(), parentItemID, childItemID, state, description.ToSql());
      ExecSQL(sql);
      string sql_CheckMsgUserState = @"SELECT top 1 * FROM tblTaskPLC2WCS 
                                              WHERE TelegramType = {0}
                                              AND PositionID = {1}
                                              ORDER BY ID desc".FormatWith(201, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckMsgUserState);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("Set stacking result could not be created");
        return -1;
      }
      return dt.Rows[0].GetInteger("ID", -1);
    }

    public int SendPalletAtLabelingPosition(int PLC_ItemID, int ProductQuantity, int ProductionOrderID, int ProductType)
    {
      string sql_PalletAtLabelingPosition = @"EXEC usp_P2W_CreatePalletAtLabelingPosition {0}, {1}, {2}, {3}, {4}, {5}
                               ".FormatWith(communicationControllerSystem.ID, plc_PositionID.ToSql(), PLC_ItemID, ProductQuantity, ProductionOrderID, ProductType);
      ExecSQL(sql_PalletAtLabelingPosition);
      string sql_CheckMsgUserState = @"SELECT TOP 1 * FROM tblTaskPLC2WCS 
                                               WHERE TelegramType = {0}
                                               AND PositionID = {1}
                                               ORDER BY ID DESC".FormatWith(10, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckMsgUserState);
      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("PalletAtLabelingPosition could not be created");
        return -1;
      }
      return dt.Rows[0].GetInteger("ID", -1);
    }

    public int SendConsumeUnit(int PLC_ItemID, int ProductQuantity, int Status)
    {
      string sql_ConsumeUnit = @"EXEC usp_P2W_CreateConsumeUnitItem {0}, {1}, {2}, {3}, {4}
                                ".FormatWith(communicationControllerSystem.ID, plc_PositionID.ToSql(), PLC_ItemID, ProductQuantity, Status);
      ExecSQL(sql_ConsumeUnit);

      string sql_CheckMsgUserState = @"SELECT TOP 1 * FROM tblTaskPLC2WCS 
                                       WHERE TelegramType = {0}
                                       AND PositionID = {1}
                                       ORDER BY ID DESC".FormatWith(310, plc_PositionID.ToSql());
      DataTable dt = GetDataTable(sql_CheckMsgUserState);

      if (dt is null || dt.Rows.Count == 0)
      {
        Assert.Fail("ConsumeUnit could not be created");
        return -1;
      }
      return dt.Rows[0].GetInteger("ID", -1);
    }

    public DbItem CreatePallet(string GRAI, int LocationID, int JobID, int PLC_ItemID, int FinalDestinationSystemID = -1, int ParentItemID = -1) //TODO - need some refining
    {
      DbItem PalletItem = new DbItem();
      string UniqueID = "PAL" + DateTime.Now.Ticks.ToString();
      bool doUpdate = false;
      try
      {
        if (GRAI != "")
        {
          DbItem ExistingPalletItem = api.Data.DbItem.Load.ByUniqueID(GRAI);
          if (ExistingPalletItem != null)
          {
            //remove all linked items
            List<DbItem> LinkedItems = api.Data.DbItem.GetList.ForParentItemID(ExistingPalletItem.ID);
            foreach (DbItem li in LinkedItems)
            {
              li.ParentItemID = -1;
              api.Data.DbItem.Save.UpdateExisting(li);
            }
            PalletItem = ExistingPalletItem;
            doUpdate = true;
          }
          UniqueID = GRAI;
        }
        DbJob job = api.Data.DbJob.Load.ByID(JobID).ThrowIfNull("Job not found");
        DbProductSet ProductSet = api.Data.DbProductSet.Load.ByKey("PS.PALLET").ThrowIfNull("Pallet ProductSet not found");
        DbProduct PalletizedProduct = api.Data.DbProduct.Load.ByID(job.ProductID).ThrowIfNull("Job Product ID not found");
        int PalletProductID = PalletizedProduct.Attribute05.AsInt(-1); //The product master data of the product being produced has the pallet ype in Attribute05
        DbProduct PalletProduct = api.Data.DbProduct.Load.ByID(PalletProductID).ThrowIfNull("Pallet Type not found");
        string PLC_ItemReference = lineReference + "." + PLC_ItemID;
        PalletItem.UniqueID = UniqueID;
        PalletItem.ParentItemID = ParentItemID;
        PalletItem.ItemDefinitionID = palletItemDefinitionID;
        PalletItem.JobID = job.ID;
        PalletItem.ProductID = PalletProduct.ID;
        PalletItem.LocationID = LocationID;
        PalletItem.Attribute01 = PLC_ItemReference;
        PalletItem.Attribute03 = DateTimeOffset.Now.ToString();
        PalletItem.Attribute18 = DateTimeOffset.Now.ToString();
        PalletItem.Quantity = 1;
        PalletItem.ValidFromDateTime = DateTimeOffset.Now;
        PalletItem.Attribute17 = DateTimeOffset.Now.ToString();
        PalletItem.UserState = 1;
        PalletItem.Attribute20 = "1";
        PalletItem.Attribute16 = FinalDestinationSystemID.ToString();
        Result<DbItem> result;
        if (doUpdate)
          result = api.Data.DbItem.Save.UpdateExisting(PalletItem);
        else
          result = api.Data.DbItem.Save.InsertAsNew(PalletItem);
        if (result.Success)
          PalletItem = result.Return;
        else
        {
          string errormsg = "";
          foreach (ResultMessage msg in result.Messages)
          {
            errormsg = errormsg + msg.Message + "; ";
          }
          throw new Exception(errormsg);
        }
      }
      catch (Exception ex)
      {
        Assert.Fail("CreateNewPalletItem() ERROR: " + ex.Message);
        PalletItem = null;
      }
      return PalletItem;
    }

    public DbItem CreateProduct(int LocationID, int JobID, int PLC_ItemID, int PalletItemID = -1, int FinalDestinationSystemID = -1, int Quantity = 1) //TODO - need some refining
    {
      DbItem ProductItem = new DbItem();
      bool doUpdate = false;
      try
      {
        DbJob job = api.Data.DbJob.Load.ByID(JobID).ThrowIfNull("Job not found");
        DbProductSet ProductSet = api.Data.DbProductSet.Load.ByKey("PS.PALLET").ThrowIfNull("Pallet ProductSet not found");
        DbProduct PalletizedProduct = api.Data.DbProduct.Load.ByID(job.ProductID).ThrowIfNull("Job Product ID not found");
        string PLC_ItemReference = lineReference + "." + PLC_ItemID;
        ProductItem.UniqueID = GetNextSSCC();
        ProductItem.ParentItemID = PalletItemID;
        ProductItem.ItemDefinitionID = productItemDefinitionID;
        ProductItem.JobID = job.ID;
        ProductItem.ProductID = PalletizedProduct.ID;
        ProductItem.LocationID = LocationID;
        ProductItem.Attribute01 = PLC_ItemReference;
        ProductItem.Attribute03 = DateTimeOffset.Now.ToString();
        ProductItem.Attribute18 = DateTimeOffset.Now.ToString();
        ProductItem.Quantity = Quantity;
        ProductItem.ValidFromDateTime = DateTimeOffset.Now;
        ProductItem.Attribute17 = DateTimeOffset.Now.ToString();
        ProductItem.UserState = 1;
        ProductItem.Attribute20 = "1";
        ProductItem.Attribute16 = FinalDestinationSystemID.ToString();
        Result<DbItem> result;
        if (doUpdate)
          result = api.Data.DbItem.Save.UpdateExisting(ProductItem);
        else
          result = api.Data.DbItem.Save.InsertAsNew(ProductItem);
        if (result.Success)
          ProductItem = result.Return;
        else
        {
          string errormsg = "";
          foreach (ResultMessage msg in result.Messages)
          {
            errormsg = errormsg + msg.Message + "; ";
          }
          throw new Exception(errormsg);
        }
      }
      catch (Exception ex)
      {
        Assert.Fail("CreateNewProductItem() ERROR: " + ex.Message);
        ProductItem = null;
      }
      return ProductItem;
    }

    public DbItem SetProductItemSSCC(DbItem ProductItem, string SSCC)
    {
      if (ProductItem != null)
      {
        ProductItem.UniqueID = SSCC;
        ProductItem = api.Data.DbItem.Save.UpdateExisting(ProductItem).Return;
      }

      return ProductItem;
    }

    public DbJobSystemActual CreateJobSystemActual(int JobID, int JobSystemID)
    {
      DbJobSystemActual JobSystemActual;
      try
      {
        string sql_GetRecentRunningJob = @"SELECT TOP 1 * FROM tJobSystemActual 
                                           WHERE JobID = {0} AND SystemID = {1}
                                           ORDER BY ID DESC".FormatWith(JobID, JobSystemID);
        JobSystemActual = api.Data.DbJobSystemActual.Load.WithSql(sql_GetRecentRunningJob);
        if (JobSystemActual != null)
        {
          string sql_ResetRunningJob = "UPDATE tJobSystemActual SET EndDateTime = NULL WHERE ID = {0}".FormatWith(JobSystemActual.ID);
          ExecSQL(sql_ResetRunningJob);
          return JobSystemActual;
        }

        JobSystemActual = new DbJobSystemActual();
        JobSystemActual.JobID = JobID;
        JobSystemActual.SystemID = JobSystemID;
        JobSystemActual.StartDateTime = DateTimeOffset.Now;
        JobSystemActual.Date = DateTime.Now;
        Result<DbJobSystemActual> result;
        result = api.Data.DbJobSystemActual.Save.InsertAsNew(JobSystemActual);
        if (result.Success)
          JobSystemActual = result.Return;
        else
        {
          string errormsg = "";
          foreach (ResultMessage msg in result.Messages)
          {
            errormsg = errormsg + msg.Message + "; ";
          }
          throw new Exception(errormsg);
        }
      }
      catch (Exception ex)
      {
        Assert.Fail("CreateJobSystemActual() ERROR: " + ex.Message);
        JobSystemActual = null;
      }
      return JobSystemActual;
    }

    public DbTask CreateUserTask(int TaskDefinitionID)
    {
      DbTask UserTask = new DbTask();
      try
      {
        UserTask.TaskDefinitionID = TaskDefinitionID;
        UserTask.CreatedDateTime = DateTimeOffset.Now;
        UserTask.Date = DateTime.Now;
        UserTask.UserState = (int)TaskUserState.NEW;
        UserTask.PassFail = PassFail.Unknown;

        Result<DbTask> result;
        result = api.Data.DbTask.Save.InsertAsNew(UserTask);
        if (result.Success)
          UserTask = result.Return;
        else
        {
          string errormsg = "";
          foreach (ResultMessage msg in result.Messages)
          {
            errormsg = errormsg + msg.Message + "; ";
          }
          throw new Exception(errormsg);
        }
      }
      catch (Exception ex)
      {
        Assert.Fail("CreateUserTask() ERROR: " + ex.Message);
        UserTask = null;
      }
      return UserTask;

    }

    public void SetRFIDTag(string TagPrefix, string TagValue)
    {
      api.Tags.UpdateVirtualTagByName(TagPrefix + "LastReadTag", TagValue);
    }

    public void SetTagValue(string TagName, string TagValue)
    {
      api.Tags.UpdateVirtualTagByName(TagName, TagValue);
    }

    public string GetTagValue(string TagName)
    {
      return api.Tags.Load.ByName(TagName)?.Value;
    }

    private string GetNextSSCC()
    {
      string sql = @"DECLARE @SSCC varchar(50) 
					 EXEC usp_GEN_GetSSCC @SSCC OUTPUT
					 SELECT @SSCC";
      string sscc = ExecSQLString(sql);
      return sscc;
    }

    public DbItem GetProductItem(DbItem PalletItem)
    {
      DbItem productItem = api.Data.DbItem.Load.WithSql("SELECT * FROM tItem WHERE ParentItemID = {0} AND ItemDefinitionID = 11".FormatWith(PalletItem.ID));
      return productItem;
    }

    public DbItem GetPalletItemByPLCItemID(int PLCItemID)
    {
      string sqlGetPalletItem = @"SELECT TOP 1 * FROM tItem 
                                  WHERE Attribute01 LIKE '%.{0}' 
                                  AND ItemDefinitionID = 10".FormatWith(PLCItemID.ToString());
      DbItem palletItem = api.Data.DbItem.Load.WithSql(sqlGetPalletItem);
      return palletItem;
    }

    public DbItem GetPalletItemByGRAI(string GRAI)
    {
      DbItem PalletItem;
      string sql_GetPalletItem = "SELECT ID FROM tItem WHERE UniqueID = {0} AND ItemDefinitionID = {1}".FormatWith(GRAI.ToSql(), palletItemDefinitionID);
      int PalletItemID = ExecSQLInt(sql_GetPalletItem);
      PalletItem = api.Data.DbItem.Load.ByID(PalletItemID);
      return PalletItem;
    }

    public string GetFOByJobID(int JobID)
    {
      DbJob job = api.Data.DbJob.Load.ByID(JobID);
      Assert.IsNotNull(job);
      return job.Name;
    }

    public string GetProductCodeFromJob(int JobID)
    {
      DbJob job = api.Data.DbJob.Load.ByID(JobID);
      Assert.IsNotNull(job);
      DbProduct product = api.Data.DbProduct.Load.ByID(job.ProductID);
      Assert.IsNotNull(product);
      return product.ProductCode;
    }

    public DbTask GetUserTaskByTaskDefinitionID(int TaskDefinitionID)
    {
      string sql_GetUserTask = @"SELECT TOP 1 * FROM tTask
                                 WHERE TaskDefinitionID = {0}".FormatWith(TaskDefinitionID);
      DbTask UserTask = api.Data.DbTask.Load.WithSql(sql_GetUserTask);
      return UserTask;
    }

    public DbTask GetUserTaskFromAtPositionTask(int AtPositionTaskID)
    {
      string sql_GetUserTask = @"SELECT TOP 1 UserTaskID FROM tblTaskPLC2WCS 
                                 WHERE ID = {0}".FormatWith(AtPositionTaskID);
      int UserTaskID = ExecSQLInt(sql_GetUserTask);
      DbTask UserTask = api.Data.DbTask.Load.ByID(UserTaskID);
      return UserTask;
    }

    public DbTask GetPrintTaskFromPLCMessage(int PLCMessageTaskID)
    {
      string sql_GetPrintTask = @"SELECT TOP 1 PrintTaskID FROM tblTaskPLC2WCS 
                                 WHERE ID = {0}".FormatWith(PLCMessageTaskID);
      int PrintTaskID = ExecSQLInt(sql_GetPrintTask);
      DbTask PrintTask = api.Data.DbTask.Load.ByID(PrintTaskID);
      return PrintTask;
    }

    public void UpdateTaskUserState(int TaskID, int UserState, PassFail PassFail, string Notes = "")
    {
      string sql_UpdateTaskUserState = @"UPDATE tTask 
                                         SET UserState = {0}, 
                                             PassFail = {1}, 
                                             Notes = {2} 
                                         WHERE ID = {3}
                                        ".FormatWith(UserState, PassFail.ToSql(), Notes.ToSql(), TaskID);
      ExecSQL(sql_UpdateTaskUserState);
    }

    public DbItem GetProductItemFromTask(int TaskID)
    {
      DbTask task = api.Data.DbTask.Load.ByID(TaskID);
      Assert.IsNotNull(task);
      DbItem item = api.Data.DbItem.Load.ByID(task.ItemID);
      return item;
    }

    public void RemoveUserTaskFromAtPositionTask(int AtPositionTaskID)
    {
      string sql_GetUserTask = @"UPDATE tblTaskPLC2WCS 
                                       SET UserTaskID = NULL
                                        WHERE ID = {0}".FormatWith(AtPositionTaskID);
      ExecSQL(sql_GetUserTask);
    }

    public void CompleteUserTask(DbTask UserTask, PassFail PassFail)
    {
      UserTask.CompletedDateTime = DateTimeOffset.Now;
      UserTask.UserState = (int)TaskUserState.COMPLETE;
      UserTask.PassFail = PassFail;
      api.Data.DbTask.Save.UpdateExisting(UserTask);
    }

    public void CancelUserTask(DbTask UserTask)
    {
      UserTask.CompletedDateTime = DateTimeOffset.Now;
      UserTask.UserState = (int)TaskUserState.CANCELLED;
      UserTask.PassFail = PassFail.Fail;
      api.Data.DbTask.Save.UpdateExisting(UserTask);
    }

    public DbProduct GetProduct(int JobID)
    {
      DbJob job = api.Data.DbJob.Load.ByID(JobID);
      if (job == null) Assert.Fail("Job not found - JobID: " + JobID);
      DbProduct product = api.Data.DbProduct.Load.ByID(job.ProductID);
      if (product == null) Assert.Fail("Product not found - ProductID: " + job.ProductID);
      return product;
    }

    public void ClearDB()
    {
      string sql_CleanupDB = @"DELETE FROM tblTaskPLC2WCS
                              DELETE FROM tblTaskWCS2PLC
                              DELETE FROM tEvent
                              DELETE FROM tTask
                              DELETE FROM tItemLog
                              DELETE FROM tItemSourceItem
                              DELETE FROM tItem
                              DELETE FROM tMaterialUseActual
                              TRUNCATE TABLE tblProductRouting
                              DELETE FROM tblProductRoutingSetup";
      ExecSQL(sql_CleanupDB);
      string sql_CompleteJobSystemActual = @"UPDATE tJobSystemActual
                                           SET EndDateTime = SYSDATETIMEOFFSET()
                                           WHERE EndDateTime IS NULL";
      ExecSQL(sql_CompleteJobSystemActual);
    }

    public bool RunTask(int AtPositionTaskID, int _LimitCounter = 10, bool FailOnOverflow = true)
    {
      int LimitCounter = 0;
      bool TaskMessageComplete = false;
      while (LimitCounter < _LimitCounter && !TaskMessageComplete)
      {
        TaskMessageComplete = GetTaskStateComplete(AtPositionTaskID);
        if (TaskMessageComplete)
        {
          break;
        }
        Conveyor.Execute();
        LimitCounter++;
      }
      if (!TaskMessageComplete && FailOnOverflow) Assert.Fail("Task did not complete in time.");
      return TaskMessageComplete;
    }

    public DbLocation GetLocationByID(int LocationID)
    {
      return api.Data.DbLocation.Load.ByID(LocationID);
    }

    public DbProduct GetProductByID(int ProductID)
    {
      return api.Data.DbProduct.Load.ByID(ProductID);
    }

    public int GetEjectionLaneSystemID()
    {
      string sqlGetEjectLane = @"SELECT TOP 1 s.ID FROM tSystem s 
INNER JOIN viewCustomPropertySystem v ON s.ID = v.ID
WHERE s.AreaID = {0} 
AND s.SystemTypeID = (SELECT ID FROM tSystemType WHERE [Key] = 'ST.CONV.BUFFER')
AND v.[CPS.CONV.ROUTING.CP.CONV.EJECTIONLANE] = 1".FormatWith(routingAreaID);
      int EjectLaneSystemID = ExecSQLInt(sqlGetEjectLane);
      return EjectLaneSystemID;
    }
  }
}
