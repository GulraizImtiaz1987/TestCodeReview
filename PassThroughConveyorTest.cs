using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ConveyorProcessLogicTest
{
  public class PassThroughConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 246; // Hooding PGV Passthrough Conveyor
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
    public void PassThroughConveyorGoodFlow_0()
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
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PassThroughConveyorGoodFlow_3()
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
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PassThroughConveyorGoodFlow_1()
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
        string PLCItemLineReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PassThroughConveyorGoodFlow_2()
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
    public void PassThroughConveyorGoodFlow_4()
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
        string PLCItemLineReference = "";
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void PassThroughConveyorBadFlow_0_NoPallet()
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
    public void PassThroughConveyorBadFlow_3_NoPallet()
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
    public void PassThroughConveyorBadFlow_0_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PassThroughConveyorBadFlow_3_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference, ejectDestinationSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion Bad Flows
  }
}