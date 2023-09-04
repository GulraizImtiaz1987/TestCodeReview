using ETS.Core.Api.Models.Data;
using NUnit.Framework;

namespace ConveyorProcessLogicTest
{
  internal class GroupingConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string Login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 154;

    DbItem PalletItem;
    DbItem ProductItem;
    DbItem WaitingPalletItem;
    DbItem WaitingProductItem;
    const int ParentPLC_ItemID = 1;
    const int ChildPLC_ItemID = 2;
    const int JobIDWithGrouping = 3684;
    const int JobIDNoGrouping = 3727;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      Init(systemID, server, database, Login, password);

      //Init(systemID_Grouping, server, database, Login, password);
      //GroupBufferConveyor.Init(systemID_GroupBuffer, server, database, Login, password);
    }

    [SetUp]
    public void Setup()
    {
      ClearDB();
    }

    #region Good Flows

    [Test]
    public void GroupingConveyorGoodFlow_0_Grouping_NoPalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_3_Grouping_NoPalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_0_Grouping_GroupPalletsOK()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      RunTask(AtPositionTaskID, 4, false);

      SendGroupingResult(ParentPLC_ItemID, (int)GroupingResultState.ACCEPTED);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckGrouped(WaitingPalletItem.ID, PalletItem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(211, TaskUserState.COMPLETE);
        CheckMessagePassFail(211, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_3_Grouping_GroupPalletsOK()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      RunTask(AtPositionTaskID, 4, false);

      SendGroupingResult(ParentPLC_ItemID, (int)GroupingResultState.ACCEPTED);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckGrouped(WaitingPalletItem.ID, PalletItem.ID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(211, TaskUserState.COMPLETE);
        CheckMessagePassFail(211, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_0_Grouping_GroupPalletsFail()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      RunTask(AtPositionTaskID, 4, false);

      SendGroupingResult(ParentPLC_ItemID, (int)GroupingResultState.FAILED);

      RunTask(AtPositionTaskID, 2, false);

      // Clear buffer
      RemovePalletsFromConveyor(nextLinkedSystemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckGrouped(WaitingPalletItem.ID, PalletItem.ID, true);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(211, TaskUserState.COMPLETE);
        CheckMessagePassFail(211, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_3_Grouping_GroupPalletsFail()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDWithGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDWithGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      RunTask(AtPositionTaskID, 4, false);

      SendGroupingResult(ParentPLC_ItemID, (int)GroupingResultState.FAILED);

      RunTask(AtPositionTaskID, 2, false);

      // Clear buffer
      RemovePalletsFromConveyor(nextLinkedSystemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckGrouped(WaitingPalletItem.ID, PalletItem.ID, true);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(211, TaskUserState.COMPLETE);
        CheckMessagePassFail(211, TaskPassFail.PASS);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_0_NoGrouping_NoPalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, ChildPLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_3_NoGrouping_NoPalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, ChildPLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_0_NoGrouping_PalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      RunTask(AtPositionTaskID, 4, false);

      RemovePalletsFromConveyor(nextLinkedSystemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_3_NoGrouping_PalletAtBuffer()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, ChildPLC_ItemID, PalletItem.ID);
      WaitingPalletItem = CreatePallet("", nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID);
      WaitingProductItem = CreateProduct(nextConveyorSystem.LocationID, JobIDWithGrouping, ParentPLC_ItemID, WaitingPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      RunTask(AtPositionTaskID, 4, false);

      RemovePalletsFromConveyor(nextLinkedSystemID);

      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_1()
    {
      int PLC_ItemID = 1;
      int JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, string.Empty);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, string.Empty);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_2()
    {
      int PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 2);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorGoodFlow_4()
    {
      int PLC_ItemID = 1;
      int JobID = 15478;

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

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void GroupingConveyorBadFlow_0_InvalidPCLItemID()
    {
      int PLC_ItemID = 0;
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
    public void GroupingConveyorBadFlow_0_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void GroupingConveyorBadFlow_3_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void GroupingConveyorBadFlow_0_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingConveyorBadFlow_3_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, ChildPLC_ItemID);

      int AtPositionTaskID = SendAtPosition(ChildPLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + ChildPLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, nextLinkedSystemID, PLCReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Bad Flows
  }
}
