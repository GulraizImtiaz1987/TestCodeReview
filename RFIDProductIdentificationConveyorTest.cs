using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ConveyorProcessLogicTest
{
  public class RFIDProductIdentificationConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 153; // Docks RFID Conveyor
    DbItem PalletItem;
    DbItem ProductItem;
    DbItem PalletItemStacked;
    DbItem ProductItemStacked;
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
    public void RFIDProductIdentificationConveyorGoodFlow_0()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_3()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_0_Stacked()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItem.ID);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, PalletItemStacked.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_3_Stacked()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItem.ID);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, PalletItemStacked.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_1()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

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
    public void RFIDProductIdentificationConveyorGoodFlow_1_Stacked()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference, -1, PalletItem.ID);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, PalletItemStacked.ID, warehouseLocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_2()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_4()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference, -1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorGoodFlow_4_Stacked()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_0_InvalidPLCItemID()
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
    public void RFIDProductIdentificationConveyorBadFlow_0_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_3_RFIDNoRead()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "NoRead");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDNoReadEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_0_RFIDTimeOut()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_3_RFIDTimeOut()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);

      RunTask(AtPositionTaskID, 2, false);

      // Wait for timeout
      System.Threading.Thread.Sleep(rfidReaderTimeout * 1000);
      if (RunTask(AtPositionTaskID))
      {
        CheckEventCreatedByDefinitionID(RFIDTimeoutEventDefinitionID);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_0_PalletNotFound()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, PLCReference);
        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_3_PalletNotFound()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);
      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, ProductItem.UniqueID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, system.LocationID, PLCReference);
        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_0_ProductNotFound()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "123");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void RFIDProductIdentificationConveyorBadFlow_3_ProductNotFound()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      SetRFIDTag(linkedRFIDReaderSystem.TemplateTagPrefix, "123");

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows
  }
}


