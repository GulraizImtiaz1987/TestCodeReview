using ETS.Core.Api.Models.Data;
using ETS.Core.Extensions;
using NUnit.Framework;
using System.Threading;

namespace ConveyorProcessLogicTest
{
  internal class GroupBufferConveryorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 155; //CONV_020030
    DbSystem PreviousConveyorSystem;
    const int PLC_ItemID = 1;
    const int PrevPLC_ItemID = 2;
    const int JobID = 3684;
    DbItem PalletItem;
    DbItem ProductItem;
    DbItem PrevPalletItem;
    DbItem PrevProductItem;

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

    #region GroupingConveryor Good Flows

    [Test]
    public void GroupingBufferConveryorGoodFlow_0_GroupingEnabled()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PalletReference);

        int setDestinationTaskID = CheckSetDestinationExist();
        Assert.AreEqual(-1, setDestinationTaskID);
        
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_3_GroupingEnabled()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PalletReference);

        int setDestinationTaskID = CheckSetDestinationExist();
        Assert.AreEqual(-1, setDestinationTaskID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_0_GroupingDisabled()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int JobIDNoGrouping = 3727;

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, PLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PalletReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_3_GroupingDisabled()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int JobIDNoGrouping = 3727;

      PalletItem = CreatePallet("", 1, JobIDNoGrouping, PLC_ItemID);
      ProductItem = CreateProduct(1, JobIDNoGrouping, PLC_ItemID, PalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PalletReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void GroupingBufferConveyorGoodFlow_1()
    {
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
    public void GroupingBufferConveyorGoodFlow_2()
    {
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
    public void GroupingBufferConveyorGoodFlow_4()
    {
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
    public void GroupingBufferConveryorGoodFlow_CheckRelease_GroupPallets_NoRelease()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETLENGTHLIMIT", 3000);
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETWEIGHTLIMIT", 1000);
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.GROUPRELEASETIMEOUTPERIOD", 600);
      Init(systemID, server, database, login, password);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID);
      PrevProductItem = CreateProduct(PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID, PrevPalletItem.ID);
      
      Conveyor.Execute();

      int setDestinationTaskID = CheckSetDestinationExist();
      Assert.AreEqual(-1, setDestinationTaskID);
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_CheckRelease_NoMaterialMatch_Release()
    {
      int JobID2 = 3727;
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID2, PrevPLC_ItemID);
      PrevProductItem = CreateProduct(PreviousConveyorSystem.LocationID, JobID2, PrevPLC_ItemID, PrevPalletItem.ID);

      Conveyor.Execute();

      CheckSetDestination(nextConv_PositionID);
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_CheckRelease_LengthLimitExceeded_Release()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETLENGTHLIMIT", 2000);
      Init(systemID, server, database, login, password);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID);
      PrevProductItem = CreateProduct(PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID, PrevPalletItem.ID);

      Conveyor.Execute();

      CheckSetDestination(nextConv_PositionID);

      // Revert changes
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETLENGTHLIMIT", 3000);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_CheckRelease_WeightLimitExceeded_Release()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETWEIGHTLIMIT", 500);
      Init(systemID, server, database, login, password);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID);
      PrevProductItem = CreateProduct(PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID, PrevPalletItem.ID);

      Conveyor.Execute();

      CheckSetDestination(nextConv_PositionID);

      // Revert changes
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETWEIGHTLIMIT", 1000);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_CheckRelease_TimerExpired_Release()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);
      int timeoutTime = 1;

      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETLENGTHLIMIT", 3000);
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.MAXTOTALPALLETWEIGHTLIMIT", 1000);
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.GROUPRELEASETIMEOUTPERIOD", timeoutTime);
      Init(systemID, server, database, login, password);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID);
      PrevProductItem = CreateProduct(PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID, PrevPalletItem.ID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      RunTask(AtPositionTaskID, 2, false);
      Thread.Sleep(timeoutTime * 1000);
      RunTask(AtPositionTaskID);

      CheckSetDestination(nextConv_PositionID);

      // Revert changes
      SetSystemCustomPropertyValue(system.ID, "CP.CONV.GROUPRELEASETIMEOUTPERIOD", 600);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void GroupingBufferConveryorGoodFlow_CheckRelease_PrevPalletToEject_Release()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PreviousConveyorSystem = GetPreviousConveyorSystem(system.ID);

      PalletItem = CreatePallet("", system.LocationID, JobID, PLC_ItemID);
      ProductItem = CreateProduct(system.LocationID, JobID, PLC_ItemID, PalletItem.ID);
      PrevPalletItem = CreatePallet("", PreviousConveyorSystem.LocationID, JobID, PrevPLC_ItemID, ejectDestinationSystemID);

      Conveyor.Execute();

      CheckSetDestination(nextConv_PositionID);
    }

    #endregion GroupingConveryor Good Flows

    #region GroupingConveryor Bad Flows

    [Test]
    public void GroupingBufferConveryorBadFlow_0_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void GroupingBufferConveryorBadFlow_3_NoPallet()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void GroupingBufferConveryorBadFlow_0_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void GroupingBufferConveryorBadFlow_3_NoProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PalletReference = lineReference + "." + PLC_ItemID.ToString();
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PalletReference);

        CheckSetDestination(ejectConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    #endregion GroupingConveryor Bad Flows
  }
}
