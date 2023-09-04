using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  public class ManualWorkstationConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    int systemID = 180;
    int systemIDAfterRFIDDiverter = 320;
    DbItem PalletItem;
    DbItem ProductItem;
    int PLC_ItemID;
    int JobID;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Init(systemID, server, database, login, password);
    }

    [SetUp]
    public void Setup()
    {
      ClearDB();
    }

    #region Good Flows

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_UserTaskPass()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, systemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);
        
        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_UserTaskPass()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, systemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);
        
        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_UserTaskFail()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, systemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        
        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_UserTaskFail()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, systemID);;

      // Trigger process
      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_NoProductItemUserTaskPass()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_NoProductItemUserTaskPass()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_NoProductItemUserTaskFail()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_NoProductItemUserTaskFail()
    {
      PLC_ItemID = 1;
      JobID = 4161;
      int TaskDefinitionID = GetTaskDefinition("TD.REINTRODUCEDPALLETREJECTED", systemID).ID;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(TaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_PassThrough()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_PassThrough()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_RFIDDiverterUserTaskPass()
    {
      Init(systemIDAfterRFIDDiverter, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      DbSystem RFIDDiverterConveyor = GetPreviousDiverterConveyorSystem(systemIDAfterRFIDDiverter);
      int TaskDefinitionID = GetTaskDefinition("TD.RFID.MANUALTASK", RFIDDiverterConveyor.ID).ID;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, system.ID);
      DbTask UserTask = CreateUserTask(TaskDefinitionID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert to other system
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_RFIDDiverterUserTaskPass()
    {
      Init(systemIDAfterRFIDDiverter, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);
      
      DbSystem RFIDDiverterConveyor = GetPreviousDiverterConveyorSystem(systemIDAfterRFIDDiverter);
      int TaskDefinitionID = GetTaskDefinition("TD.RFID.MANUALTASK", RFIDDiverterConveyor.ID).ID;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, system.ID);
      DbTask UserTask = CreateUserTask(TaskDefinitionID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert to other system
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_0_RFIDDiverterUserTaskFail()
    {
      Init(systemIDAfterRFIDDiverter, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      DbSystem RFIDDiverterConveyor = GetPreviousDiverterConveyorSystem(systemIDAfterRFIDDiverter);
      int TaskDefinitionID = GetTaskDefinition("TD.RFID.MANUALTASK", RFIDDiverterConveyor.ID).ID;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, system.ID);
      DbTask UserTask = CreateUserTask(TaskDefinitionID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert to other system
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void ManualWorkstationConveyorGoodFlow_3_RFIDDiverterUserTaskFail()
    {
      Init(systemIDAfterRFIDDiverter, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      DbSystem RFIDDiverterConveyor = GetPreviousDiverterConveyorSystem(systemIDAfterRFIDDiverter);
      int TaskDefinitionID = GetTaskDefinition("TD.RFID.MANUALTASK", RFIDDiverterConveyor.ID).ID;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, system.ID);
      DbTask UserTask = CreateUserTask(TaskDefinitionID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Complete User Task
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        int UserTaskID = CheckUserTask(TaskDefinitionID, AtPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert to other system
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void ManualWorkstationConveyor_1()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void ManualWorkstationConveyor_4()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows


    #region Bad Flows

    [Test]
    public void ManualWorkstationConveyorBadFlow_0_InvalidPLCItemID()
    {
      PLC_ItemID = 0;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void ManualWorkstationConveyorBadFlow_0_NoPalletItem()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    } 

    [Test]
    public void ManualWorkstationConveyorBadFlow_3_NoPalletItem()
    {
      PLC_ItemID = 1;
      JobID = 4161;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }
    
    #endregion Bad Flows
  }
}
