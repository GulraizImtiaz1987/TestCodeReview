using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ConveyorProcessLogicTest
{
  internal class HooderConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 231; //Hooding Station
    DbItem PalletItem;
    DbItem ProductItem;
    DbItem PalletItemStacked;
    DbItem ProductItemStacked;
    DbItem PalletItemGrouped;
    DbItem ProductItemGrouped;
    DbItem PalletItemGroupedStacked;
    DbItem ProductItemGroupedStacked;
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

    #region HooderConveyor State 0 & 3

    #region HooderConveyor State 0 & 3 Good Flow

    [Test]
    public void HooderConveyorGoodFlow_0()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID, systemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void HooderConveyorGoodFlow_3()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID, systemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion HooderConveyor State 0 & 3 Good Flow

    #region HooderConveyor State 0 & 3 Bad Flow

    // PLCItemID = 0
    [Test]
    public void HooderConveyorBadFlow_0_NoPLCItemID()
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

    // Pallet final destination not set to the systemID
    [Test]
    public void HooderConveyorBadFlow_0_PalletFinDestIncorrect()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void HooderConveyorBadFlow_3_PalletFinDestIncorrect()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    //No Pallet found
    [Test]
    public void HooderConveyorBadFlow_0_NoPallet()
    {
      PLC_ItemID = 1;

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
    public void HooderConveyorBadFlow_3_NoPallet()
    {
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    // No Product Found
    [Test]
    public void HooderConveyorBadFlow_0_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID, systemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void HooderConveyorBadFlow_3_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID, systemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion HooderConveyor State 0 & 3 Bad Flow

    #endregion HooderConveyorState 0 & 3

    #region HooderConveyor State 1

    [Test]
    public void HooderConveyorGoodFlow_1()
    {
      PLC_ItemID = 1;
      JobID = 15478;

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

    #endregion HooderConveyor State 1

    #region HooderConveyor State 2

    [Test]
    public void HooderConveyorGoodFlow_2()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion HooderConveyor State 2

    #region HooderConveyor State 4

    [Test]
    public void HooderConveyorGoodFlow_4()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

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
    public void HooderConveyorGoodFlow_4_Stacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID, -1, PalletItem.ID);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference, -1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void HooderConveyorGoodFlow_4_Grouped()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemGrouped = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItemGrouped = CreateProduct(1, JobID, PLC_ItemID, PalletItemGrouped.ID);
      GroupPallets(PalletItem, PalletItemGrouped);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;

        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference, -1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void HooderConveyorGoodFlow_4_GroupedAndStacked()
    {
      PLC_ItemID = 1;
      int PLC_ItemID2 = 2;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID, -1, PalletItem.ID);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);
      PalletItemGrouped = CreatePallet("", 1, JobID, PLC_ItemID2);
      ProductItemGrouped = CreateProduct(1, JobID, PLC_ItemID2, PalletItemGrouped.ID);
      PalletItemGroupedStacked = CreatePallet("", 1, JobID, PLC_ItemID2, -1, PalletItemGrouped.ID);
      ProductItemGroupedStacked = CreateProduct(1, JobID, PLC_ItemID2, PalletItemGroupedStacked.ID);
      GroupPallets(PalletItem, PalletItemGrouped);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference, -1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemGroupedStacked.ID, PalletItemGroupedStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemGroupedStacked.ID, ProductItemGroupedStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion HooderConveyor State 4
  }
}
