using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;

namespace ConveyorProcessLogicTest
{
  internal class StackerConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 234; // Hooder PGV Stacker
    DbItem PalletItem;
    DbItem ProductItem;
    DbItem ParentPalletItem;
    DbItem ParentProductItem;
    DbItem ChildPalletItem;
    DbItem ChildProductItem;
    int PLC_ItemID;
    int Parent_PLC_ItemID;
    int Child_PLC_ItemID;
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

    #region StackingResult Good Flows

    [Test]
    public void StackerConveryorGoodFlow_StackingResult_0_StackPallets()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ParentPLCItemLineReference = lineReference + "." + Parent_PLC_ItemID;
      string ChildPLCItemLineReference = lineReference + "." + Child_PLC_ItemID;
      JobID = 15478;

      ParentPalletItem = CreatePallet("", 1, JobID, Parent_PLC_ItemID);
      ParentProductItem = CreateProduct(1, JobID, Parent_PLC_ItemID, ParentPalletItem.ID);
      ChildPalletItem = CreatePallet("", 1, JobID, Child_PLC_ItemID);
      ChildProductItem = CreateProduct(1, JobID, Child_PLC_ItemID, ChildPalletItem.ID);

      // Parent Pallet
      int ParentAtPositionTaskID = SendAtPosition(Parent_PLC_ItemID, 0);
      if (RunTask(ParentAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Child Pallet
      int ChildAtPositionTaskID = SendAtPosition(Child_PLC_ItemID, 0);
      if (RunTask(ChildAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 0);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ParentPalletItem.ID, ParentPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ParentPLCItemLineReference);
        CheckProductData(ParentProductItem.ID, ParentProductItem.UniqueID, productItemDefinitionID, ParentPalletItem.ID, system.LocationID, ParentPLCItemLineReference);
        CheckPalletData(ChildPalletItem.ID, ChildPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ChildPLCItemLineReference, -1, ParentPalletItem.ID);
        CheckProductData(ChildProductItem.ID, ChildProductItem.UniqueID, productItemDefinitionID, ChildPalletItem.ID, system.LocationID, ChildPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Parent_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.PASS);
      }
    }

    [Test]
    public void StackerConveryorGoodFlow_StackingResult_1_StackPallets()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ParentPLCItemLineReference = lineReference + "." + Parent_PLC_ItemID;
      string ChildPLCItemLineReference = lineReference + "." + Child_PLC_ItemID;
      JobID = 15478;

      ParentPalletItem = CreatePallet("", 1, JobID, Parent_PLC_ItemID);
      ParentProductItem = CreateProduct(1, JobID, Parent_PLC_ItemID, ParentPalletItem.ID);
      ChildPalletItem = CreatePallet("", 1, JobID, Child_PLC_ItemID);
      ChildProductItem = CreateProduct(1, JobID, Child_PLC_ItemID, ChildPalletItem.ID);

      // Parent Pallet
      int ParentAtPositionTaskID = SendAtPosition(Parent_PLC_ItemID, 0);
      if (RunTask(ParentAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Child Pallet
      int ChildAtPositionTaskID = SendAtPosition(Child_PLC_ItemID, 0);
      if (RunTask(ChildAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 1);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ParentPalletItem.ID, ParentPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ParentPLCItemLineReference);
        CheckProductData(ParentProductItem.ID, ParentProductItem.UniqueID, productItemDefinitionID, ParentPalletItem.ID, system.LocationID, ParentPLCItemLineReference);
        CheckPalletData(ChildPalletItem.ID, ChildPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ChildPLCItemLineReference, -1, ParentPalletItem.ID);
        CheckProductData(ChildProductItem.ID, ChildProductItem.UniqueID, productItemDefinitionID, ChildPalletItem.ID, system.LocationID, ChildPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Parent_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.PASS);
      }
    }

    #endregion StackingResult Good Flows

    #region StackingResult Bad Flows

    [Test]
    public void StackerConveryorBadFlow_StackingResult_2_InvalidState()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      JobID = 15478;

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 2);
      if (RunTask(stackingResultTaskID))
      {
        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_0_NoChildPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ParentPLCItemLineReference = lineReference + "." + Parent_PLC_ItemID;
      JobID = 15478;

      ParentPalletItem = CreatePallet("", 1, JobID, Parent_PLC_ItemID);
      ParentProductItem = CreateProduct(1, JobID, Parent_PLC_ItemID, ParentPalletItem.ID);

      // Parent Pallet
      int ParentAtPositionTaskID = SendAtPosition(Parent_PLC_ItemID, 0);
      if (RunTask(ParentAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 0);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ParentPalletItem.ID, ParentPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ParentPLCItemLineReference);
        CheckProductData(ParentProductItem.ID, ParentProductItem.UniqueID, productItemDefinitionID, ParentPalletItem.ID, system.LocationID, ParentPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Parent_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_1_NoChildPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ParentPLCItemLineReference = lineReference + "." + Parent_PLC_ItemID;
      JobID = 15478;

      ParentPalletItem = CreatePallet("", 1, JobID, Parent_PLC_ItemID);
      ParentProductItem = CreateProduct(1, JobID, Parent_PLC_ItemID, ParentPalletItem.ID);

      // Parent Pallet
      int ParentAtPositionTaskID = SendAtPosition(Parent_PLC_ItemID, 0);
      if (RunTask(ParentAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 1);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ParentPalletItem.ID, ParentPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ParentPLCItemLineReference);
        CheckProductData(ParentProductItem.ID, ParentProductItem.UniqueID, productItemDefinitionID, ParentPalletItem.ID, system.LocationID, ParentPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Parent_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_0_NoParentPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ChildPLCItemLineReference = lineReference + "." + Child_PLC_ItemID;
      JobID = 15478;

      ChildPalletItem = CreatePallet("", 1, JobID, Child_PLC_ItemID);
      ChildProductItem = CreateProduct(1, JobID, Child_PLC_ItemID, ChildPalletItem.ID);

      // Child Pallet
      int ChildAtPositionTaskID = SendAtPosition(Child_PLC_ItemID, 0);
      if (RunTask(ChildAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 0);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ChildPalletItem.ID, ChildPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ChildPLCItemLineReference);
        CheckProductData(ChildProductItem.ID, ChildProductItem.UniqueID, productItemDefinitionID, ChildPalletItem.ID, system.LocationID, ChildPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Child_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_1_NoParentPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      string ChildPLCItemLineReference = lineReference + "." + Child_PLC_ItemID;
      JobID = 15478;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      ChildPalletItem = CreatePallet("", 1, JobID, Child_PLC_ItemID);
      ChildProductItem = CreateProduct(1, JobID, Child_PLC_ItemID, ChildPalletItem.ID);

      // Child Pallet
      int ChildAtPositionTaskID = SendAtPosition(Child_PLC_ItemID, 0);
      if (RunTask(ChildAtPositionTaskID))
      {
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 1);
      if (RunTask(stackingResultTaskID))
      {
        CheckPalletData(ChildPalletItem.ID, ChildPalletItem.UniqueID, palletItemDefinitionID, system.LocationID, ChildPLCItemLineReference);
        CheckProductData(ChildProductItem.ID, ChildProductItem.UniqueID, productItemDefinitionID, ChildPalletItem.ID, system.LocationID, ChildPLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);
        CheckSetDestinationItemID(Child_PLC_ItemID.ToString());

        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_0_NoPallets()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      JobID = 15478;

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 0);
      if (RunTask(stackingResultTaskID))
      {
        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_StackingResult_1_NoPallets()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      Parent_PLC_ItemID = 1;
      Child_PLC_ItemID = 2;
      JobID = 15478;

      int stackingResultTaskID = SendStackingResult(Parent_PLC_ItemID, Child_PLC_ItemID, 1);
      if (RunTask(stackingResultTaskID))
      {
        CheckMessageUserState(201, TaskUserState.COMPLETE);
        CheckMessagePassFail(201, TaskPassFail.FAIL);
      }
    }

    #endregion StackingResult Bad Flows

    #region AtPosition Good Flows

    [Test]
    public void StackerConveryorGoodFlow_AtPosition_0()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void StackerConveryorGoodFlow_AtPosition_3()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void StackerConveryorGoodFlow_AtPosition_1()
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
    public void StackerConveryorGoodFlow_AtPosition_2()
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
    public void StackerConveryorGoodFlow_AtPosition_4()
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
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion AtPosition Good Flows

    #region AtPosition Bad Flows

    [Test]
    public void StackerConveryorBadFlow_AtPosition_0_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_AtPosition_0_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_AtPosition_3_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void StackerConveryorBadFlow_AtPosition_3_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion AtPosition Bad Flows
  }
}
