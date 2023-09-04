using ETS.Core.Api.Models.Data;
using ETS.Core.Extensions;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  public class RFIDPalletIdentificationConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 174; // CONV_380
    DbItem PalletItem;
    DbItem ProductItem;
    DbJobSystemActual Jsa;
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
    public void RFIDPalletIdentificationConveyorGoodFlow_0_RFIDRequired_ExistingPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductParentItemID(ProductItem.ID, -1);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_3_RFIDRequired_ExistingPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      PalletItem = CreatePallet(GRAI, 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductParentItemID(ProductItem.ID, -1);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_0_RFIDRequired_CreatePallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_3_RFIDRequired_CreatePallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;
      string GRAI = "1234567890";

      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, GRAI);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_0_NoRFIDRequired_ExistingPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, altConveyorSystem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_3_NoRFIDRequired_ExistingPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3679;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, altConveyorSystem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_0_NoRFIDRequired_CreatePallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3679;

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
        Assert.IsNotNull(PalletItem);
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCReference, altConveyorSystem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_3_NoRFIDRequired_CreatePallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 3679;

      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        PalletItem = GetPalletItemByPLCItemID(PLC_ItemID);
        Assert.IsNotNull(PalletItem);
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, "", palletItemDefinitionID, system.LocationID, PLCReference, altConveyorSystem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_1()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

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
    public void RFIDPalletIdentificationConveyorGoodFlow_2()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorGoodFlow_4()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

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
    public void RFIDPalletIdentificationConveyorBadFlow_0_InvalidPLCItemID()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 0;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorBadFlow_0_NoActiveJob()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorBadFlow_0_RFIDTimeout()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      RunTask(AtPositionTaskID, 1, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorBadFlow_3_RFIDTimeout()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      RunTask(AtPositionTaskID, 1, false);

      // Wait for timeout
      int rfidReaderTimeout = GetSystemCustomPropertyValue(system.ID, "CPS.CONV.RFIDREADER.CP.CONV.RFIDREADERTIMEOUT").AsInt(5);
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorBadFlow_0_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDPalletIdentificationConveyorBadFlow_3_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4161;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      Jsa = CreateJobSystemActual(JobID, foSystemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows

  }
}
