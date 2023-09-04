using ETS.Core.Api.Models.Data;
using ETS.Core.Enums;
using ETS.Core.Extensions;
using NUnit.Framework;
using System;

namespace ConveyorProcessLogicTest
{
  public class PrintAndLabelingConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 178;
    DbItem PalletItem;
    DbItem ProductItem;
    int PLC_ItemID;
    int JobID;
    DbJobSystemActual Jsa;

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

    #region No RFID Required

    //No RFID Required - FO and Material supplied in PalletAtLabeling message
    //Pallet and Product do not exist - was not made at palletizer
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_NoRFIDFOCreatePallet()
    {
      PLC_ItemID = 1;
      JobID = 3679;
      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      // Get Print Task
      DbTask PrintTask = GetPrintTaskFromPLCMessage(PalletAtLabelingPositionTaskID);
      Assert.IsNotNull(PrintTask);

      //Reset PrintTask to WIP 
      UpdateTaskUserState(PrintTask.ID, 1, PassFail.Unknown);

      //Wait for task to complete - do nothing
      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      //Complete the PrintTask 
      UpdateTaskUserState(PrintTask.ID, 5, PassFail.Pass);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        ProductItem = GetProductItemFromTask(PrintTask.ID);
        Assert.IsNotNull(ProductItem);
        PalletItem = api.Data.DbItem.Load.ByID(ProductItem.ParentItemID);
        Assert.IsNotNull(PalletItem);

        string PLCItemLineReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCItemLineReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, "", productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //No RFID Required - FO and Material not supplied in PalletAtLabeling message
    //Pallet and Product do not exist - was not made at palletizer
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_NoRFIDNoFOCreatePallet()
    {
      PLC_ItemID = 1;
      JobID = 3679;

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, 0, 0);

      RunTask(PalletAtLabelingPositionTaskID, 8, false);

      // Get Print Task
      DbTask PrintTask = GetPrintTaskFromPLCMessage(PalletAtLabelingPositionTaskID);
      Assert.IsNotNull(PrintTask);

      //Reset PrintTask to WIP 
      UpdateTaskUserState(PrintTask.ID, 1, PassFail.Unknown);

      //Wait for task to complete - do nothing
      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      //Complete the PrintTask 
      UpdateTaskUserState(PrintTask.ID, 5, PassFail.Pass);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        ProductItem = GetProductItemFromTask(PrintTask.ID);
        Assert.IsNotNull(ProductItem);
        PalletItem = api.Data.DbItem.Load.ByID(ProductItem.ParentItemID);
        Assert.IsNotNull(PalletItem);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //No RFID Required - FO and Material not supplied in PalletAtLabeling message
    //Pallet and Product exist - was made at palletizer
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_NoRFIDNoFOExistingPallet()
    {
      PLC_ItemID = 1;
      JobID = 3679;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, 0, 0);

      RunTask(PalletAtLabelingPositionTaskID, 8, false);

      // Get Print Task
      DbTask PrintTask = GetPrintTaskFromPLCMessage(PalletAtLabelingPositionTaskID);
      Assert.IsNotNull(PrintTask);

      //Reset PrintTask to WIP 
      UpdateTaskUserState(PrintTask.ID, 1, PassFail.Unknown);

      //Wait for task to complete - do nothing
      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      //Complete the PrintTask 
      UpdateTaskUserState(PrintTask.ID, 5, PassFail.Pass);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        ProductItem = GetProductItemFromTask(PrintTask.ID);
        PalletItem = api.Data.DbItem.Load.ByID(ProductItem.ParentItemID);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    #endregion No RFID Required

    #region RFID Required

    //RFID Required - FO and Material not supplied in PalletAtLabeling message
    //The pallet and item already exist - so it needs to be cleared and reused
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDNoFOExistingPallet()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      string RFIDTagValue = "1000.123456.0001";
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      //Create the pallet & product item but do not use them
      DbItem oldPallet = CreatePallet(RFIDTagValue, 1, 3838, 555);
      DbItem oldProduct = CreateProduct(1, 3838, 555, oldPallet.ID);

      SetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + "LastReadTag", RFIDTagValue);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, 0, 0);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        PalletItem = GetPalletItemByGRAI(RFIDTagValue);
        ProductItem = GetProductItem(PalletItem);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckProductParentItemID(oldProduct.ID, -1);
        Assert.AreEqual(oldPallet.ID, PalletItem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //RFID Required - FO and Material not supplied in PalletAtLabeling message
    //The pallet and item do not exist - so it needs to be created
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDNoFOCreatePallet()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      string RFIDTagValue = "1000.123456.0001";
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      SetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + "LastReadTag", RFIDTagValue);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, 0, 0);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        PalletItem = GetPalletItemByGRAI(RFIDTagValue);
        Assert.IsNotNull (PalletItem);
        ProductItem = GetProductItem(PalletItem);
        Assert.IsNotNull(ProductItem);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //RFID Required - FO and Material supplied in PalletAtLabeling message
    //The pallet and item already exists - so it need to be cleared and reused
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDFOExistingPallet()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      string RFIDTagValue = "1000.123456.0001";      

      //Create the pallet item but do not use them
      DbItem oldPallet = CreatePallet(RFIDTagValue, warehouseLocationID, 3838, 555);
      DbItem oldProduct = CreateProduct(warehouseLocationID, 3838, 555, oldPallet.ID);

      SetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + "LastReadTag", RFIDTagValue);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      RunTask(PalletAtLabelingPositionTaskID, 2, false);
      int newPalletItemID = GetPalletItemByPLCItemID(PLC_ItemID).ID;

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        PalletItem = GetPalletItemByGRAI(RFIDTagValue);
        ProductItem = GetProductItem(PalletItem);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckProductParentItemID(oldProduct.ID, -1);
        Assert.AreEqual(oldPallet.ID, PalletItem.ID);
        DbItem newPalletItem = api.Data.DbItem.Load.ByID(newPalletItemID);
        Assert.IsNull(newPalletItem);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //RFID Required - FO and Material supplied in PalletAtLabeling message
    //The pallet and item do not exist - so it need to be created
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDFOCreatePallet()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      string RFIDTagValue = "1000.123456.0001";

      SetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + "LastReadTag", RFIDTagValue);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        PalletItem = GetPalletItemByGRAI(RFIDTagValue);
        Assert.IsNotNull(PalletItem);
        ProductItem = GetProductItem(PalletItem);
        Assert.IsNotNull(ProductItem);

        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    //RFID Required 
    //The pallet and item already exist - FO supplied by PLC
    //But the RFID reader timed out and a user task is created
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDTimeout_UserTaskPass()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(PalletAtLabelingPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Pass);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        CheckSetDestination(nextConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, PalletAtLabelingPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.PASS);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.PASS);
      }
    }

    #endregion RFID Required

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void PrintAndLabelingConveyorBadFlow_InvalidPLCItemID()
    {
      PLC_ItemID = 0;
      JobID = 3679;
      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      //Trigger process
      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.FAIL);
      }
    }

    //RFID Required 
    //The pallet and item already exist - FO is supplied by PLC
    //But the RFID tag fail to read
    [Test]
    public void PrintAndLabelingConveyorBadFlow_RFIDNoRead()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      Jsa = CreateJobSystemActual(JobID, foSystemID);
      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      string RFIDTagValue = "NoRead";
      SetTagValue(linkedRFIDReaderSystem.TemplateTagPrefix + "LastReadTag", RFIDTagValue);

      //Create the pallet item but do not use them
      DbItem oldPallet = CreatePallet(RFIDTagValue, warehouseLocationID, 3838, 555);
      CreateProduct(warehouseLocationID, 3838, 555, oldPallet.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, PLC_ItemID, ProductionOrderID.AsInt(), ProductType.AsInt());

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.FAIL);
      }
    }

    //RFID Required 
    //The pallet and item already exist - FO not supplied by PLC
    //But the RFID reader timed out and a user task is created
    [Test]
    public void PrintAndLabelingConveyorGoodFlow_RFIDTimeout_UserTaskFail()
    {
      PLC_ItemID = 1;
      JobID = 15007;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      string ProductionOrderID = GetFOByJobID(JobID);
      string ProductType = GetProductCodeFromJob(JobID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int PalletAtLabelingPositionTaskID = SendPalletAtLabelingPosition(PLC_ItemID, 1, ProductionOrderID.AsInt(), ProductType.AsInt());

      RunTask(PalletAtLabelingPositionTaskID, 4, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      RunTask(PalletAtLabelingPositionTaskID, 1, false);

      // Complete User Task
      DbTask UserTask = GetUserTaskByTaskDefinitionID(ManualRFIDUserTaskDefinitionID);
      Assert.IsNotNull(UserTask);
      CompleteUserTask(UserTask, PassFail.Fail);

      if (RunTask(PalletAtLabelingPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);
        CheckEventCreatedByDefinitionID(ProcessErrorEventDefinitionID);

        int UserTaskID = CheckUserTask(ManualRFIDUserTaskDefinitionID, PalletAtLabelingPositionTaskID);
        CheckUserTaskUserState(UserTaskID, TaskUserState.COMPLETE);
        CheckUserTaskPassFail(UserTaskID, TaskPassFail.FAIL);

        CheckMessageUserState(10, TaskUserState.COMPLETE);
        CheckMessagePassFail(10, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows

  }
}
