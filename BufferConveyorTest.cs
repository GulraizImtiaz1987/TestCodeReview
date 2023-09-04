using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using System;

namespace ConveyorProcessLogicTest
{
  internal class BufferConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 167; //CONV_501 Buffer conveyor at the docks
    const int OnlyProductRowCount = 7;
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

    #region BufferConveryor State 0 & 3

    #region BufferConveryor State 0 & 3 GoodFlow

    [Test]
    public void BufferConveryorGoodFlow_0_UseAsOutletTrue()
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
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        DbLocation location = GetLocationByID(system.LocationID);
        DbLocation parentLocation = GetLocationByID(location.ParentLocationID);
        CheckPalletAtLocation(ProductItem.UniqueID, location.AltName, parentLocation.AltName);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_3_UseAsOutletTrue()
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
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        DbLocation location = GetLocationByID(system.LocationID);
        DbLocation parentLocation = GetLocationByID(location.ParentLocationID);
        CheckPalletAtLocation(ProductItem.UniqueID, location.AltName, parentLocation.AltName);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_0_UseAsOutletFalse()
    {
      SetSystemCustomPropertyValue(systemID, "USABLEASOUTFEED", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      SetSystemCustomPropertyValue(systemID, "USABLEASOUTFEED", 1);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void BufferConveryorGoodFlow_3_UseAsOutletFalse()
    {
      SetSystemCustomPropertyValue(systemID, "USABLEASOUTFEED", 0);
      Init(systemID, server, database, login, password);

      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      SetSystemCustomPropertyValue(systemID, "USABLEASOUTFEED", 1);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void BufferConveryorGoodFlow_0_Stacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
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

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_3_Stacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
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

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_0_Grouped()
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

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, PalletItemGrouped.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_3_Grouped()
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

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, PalletItemGrouped.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_0_GroupedAndStacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);
      PalletItemGrouped = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItemGrouped = CreateProduct(1, JobID, PLC_ItemID, PalletItemGrouped.ID);
      PalletItemGroupedStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemGroupedStacked.ParentItemID = PalletItemGrouped.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemGroupedStacked);
      ProductItemGroupedStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemGroupedStacked.ID);
      GroupPallets(PalletItem, PalletItemGrouped);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItem.ID);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, PalletItemStacked.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, PalletItemGrouped.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGroupedStacked.ID, PalletItemGroupedStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItemGrouped.ID);
        CheckProductData(ProductItemGroupedStacked.ID, ProductItemGroupedStacked.UniqueID, productItemDefinitionID, PalletItemGroupedStacked.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_3_GroupedAndStacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);
      PalletItemGrouped = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItemGrouped = CreateProduct(1, JobID, PLC_ItemID, PalletItemGrouped.ID);
      PalletItemGroupedStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemGroupedStacked.ParentItemID = PalletItemGrouped.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemGroupedStacked);
      ProductItemGroupedStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemGroupedStacked.ID);
      GroupPallets(PalletItem, PalletItemGrouped);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItem.ID);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, PalletItemStacked.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, PalletItemGrouped.ID, system.LocationID, PLCReference);
        CheckPalletData(PalletItemGroupedStacked.ID, PalletItemGroupedStacked.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, -1, PalletItemGrouped.ID);
        CheckProductData(ProductItemGroupedStacked.ID, ProductItemGroupedStacked.UniqueID, productItemDefinitionID, PalletItemGroupedStacked.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion BufferConveryor  State 0 & 3 Good Flow

    #region BufferConveryor State 0 & 3 Bad Flow

    [Test]
    public void BufferConveryorBadFlow_0_PLCItem0()
    {
      PLC_ItemID = 0;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void BufferConveryorBadFlow_0_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorBadFlow_3_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorBadFlow_0_NoPallet()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorBadFlow_3_NoPallet()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      ProductItem = CreateProduct(1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion BufferConveryor State 0 & 3 Bad Flow

    #endregion BufferConveryor State 0 & 3 

    #region BufferConveryor State 1

    [Test]
    public void BufferConveryorGoodFlow_1()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID, 180);
      
      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

        int tblProductRoutingRow = CheckTblProductRouting(systemID, ProductItem.ProductID, false, true);
        Assert.AreEqual(tblProductRoutingRow, OnlyProductRowCount);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion BufferConveryor State 1

    #region BufferConveryor State 2

    [Test]
    public void BufferConveryorGoodFlow_2()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, 1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, 1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion BufferConveryor State 2

    #region BufferConveryor State 4

    [Test]
    public void BufferConveryorGoodFlow_4()
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
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, string.Empty);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void BufferConveryorGoodFlow_4_GroupedAndStacked()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);
      PalletItemStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemStacked.ParentItemID = PalletItem.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemStacked);
      ProductItemStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemStacked.ID);
      PalletItemGrouped = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItemGrouped = CreateProduct(1, JobID, PLC_ItemID, PalletItemGrouped.ID);
      PalletItemGroupedStacked = CreatePallet("", 1, JobID, PLC_ItemID);
      PalletItemGroupedStacked.ParentItemID = PalletItemGrouped.ID;
      api.Data.DbItem.Save.UpdateExisting(PalletItemGroupedStacked);
      ProductItemGroupedStacked = CreateProduct(1, JobID, PLC_ItemID, PalletItemGroupedStacked.ID);

      GroupPallets(PalletItem, PalletItemGrouped);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 4);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemStacked.ID, PalletItemStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemStacked.ID, ProductItemStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemGrouped.ID, PalletItemGrouped.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItemGrouped.ID, ProductItemGrouped.UniqueID, productItemDefinitionID, -1, -1, PLCReference);
        CheckPalletData(PalletItemGroupedStacked.ID, PalletItemGroupedStacked.UniqueID, palletItemDefinitionID, -1, PLCReference, -1, -1);
        CheckProductData(ProductItemGroupedStacked.ID, ProductItemGroupedStacked.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion BufferConveryor State 4

  }
}
