using ETS.Core.Api.Models.Data;
using ETS.Core.Extensions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace ConveyorProcessLogicTest
{
  public class PalletizerConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 175;
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

    [Test]
    public void PalletizerConveyorGoodFlow_0()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      DbProduct product = GetProduct(JobID);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        int FinalDestID = product.Attribute04.AsInt(-1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_3()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 15478;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      DbProduct product = GetProduct(JobID);
      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        int FinalDestID = product.Attribute04.AsInt(-1);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_0_IntermediateProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4100;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        int FinalDestID = alternateDestinationSystemID;
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_3_IntermediateProduct()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 4100;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        int FinalDestID = alternateDestinationSystemID;
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_0_DestinationIsEject()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 8561;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_3_DestinationIsEject()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;
      JobID = 8561;
      Jsa = CreateJobSystemActual(JobID, foSystemID);

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCItemLineReference);
        DbItem ProductItem = GetProductItem(PalletItem);
        if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCItemLineReference);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_1()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);

      if (RunTask(AtPositionTaskID))
      {
        string PLCItemLineReference = "";

        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void PalletizerConveyorGoodFlow_2()
    {
      PLC_ItemID = 1;

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
    public void PalletizerConveyorGoodFlow_4()
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
        string PLCItemLineReference = "";
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, -1, PLCItemLineReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, -1, -1, PLCItemLineReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }


    //[Test]
    //public void PalletizerConveyorAtPositionState0_GoodFlow_WithoutRFID()
    //{
    //  Assert.IsNotNull(api);
    //  Assert.IsNotNull(Conveyor);

    //  PLC_ItemID = 1;
    //  JobID = 15478;
    //  //Setup test
    //  DbProduct product = GetProduct(JobID);
    //  PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

    //  //Trigger process
    //  int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
    //  if (RunTask(AtPositionTaskID))
    //  {
    //    string PLCItemLineReference = lineReference + "." + PLC_ItemID;
    //    CheckPalletData(PalletItem.ID, PalletItem.UniqueID, 10, system.LocationID, PLCItemLineReference);
    //    DbItem ProductItem = GetProductItem(PalletItem);
    //    if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
    //    int FinalDestID = GetProductCustomPropertyValue(product.ID, "CPS.PRODUCT.CP.DESTINATIONLOCATIONID").AsInt(-1);
    //    CheckProductData(ProductItem.ID, ProductItem.UniqueID, 11, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);
    //    //CheckSetDestination
    //    CheckSetDestination(nextConv_PositionID);
    //    CheckMessageUserState(120, TaskUserState.COMPLETE);
    //  }
    //}

    //[Test]
    //public void PalletizerConveyorAtPositionState0_GoodFlow_WithRFID()
    //{
    //  Assert.IsNotNull(api);
    //  Assert.IsNotNull(Conveyor);

    //  PLC_ItemID = 1;
    //  JobID = 15478;
    //  //Setup test
    //  DbProduct product = GetProduct(JobID);
    //  string RFIDTagValue = "3500003E8033400000001D2C";
    //  PalletItem = CreatePallet(RFIDTagValue, 1, JobID, PLC_ItemID);

    //  //Trigger process
    //  int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
    //  if (RunTask(AtPositionTaskID))
    //  {
    //    string PLCItemLineReference = lineReference + "." + PLC_ItemID;
    //    CheckPalletData(PalletItem.ID, RFIDTagValue, 10, system.LocationID, PLCItemLineReference);
    //    DbItem ProductItem = GetProductItem(PalletItem);
    //    if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
    //    int FinalDestID = GetProductCustomPropertyValue(product.ID, "CPS.PRODUCT.CP.DESTINATIONLOCATIONID").AsInt(-1);
    //    CheckProductData(ProductItem.ID, ProductItem.UniqueID, 11, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);
    //    //CheckSetDestination
    //    CheckSetDestination(nextConv_PositionID);
    //    CheckMessageUserState(120, TaskUserState.COMPLETE);
    //  }
    //}

    //public void PalletizerConveyorAtPositionState3_GoodFlow_WithoutRFID()
    //{
    //  Assert.IsNotNull(api);
    //  Assert.IsNotNull(Conveyor);

    //  PLC_ItemID = 1;
    //  JobID = 15478;
    //  //Setup test
    //  DbProduct product = GetProduct(JobID);
    //  PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

    //  //Trigger process
    //  int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
    //  if (RunTask(AtPositionTaskID))
    //  {
    //    string PLCItemLineReference = lineReference + "." + PLC_ItemID;
    //    CheckPalletData(PalletItem.ID, PalletItem.UniqueID, 10, system.LocationID, PLCItemLineReference);
    //    DbItem ProductItem = GetProductItem(PalletItem);
    //    if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
    //    int FinalDestID = GetProductCustomPropertyValue(product.ID, "CPS.PRODUCT.CP.DESTINATIONLOCATIONID").AsInt(-1);
    //    CheckProductData(ProductItem.ID, ProductItem.UniqueID, 11, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);
    //    //CheckSetDestination
    //    CheckSetDestination(nextConv_PositionID);
    //    CheckMessageUserState(120, TaskUserState.COMPLETE);
    //  }
    //}

    //[Test]
    //public void PalletizerConveyorAtPositionState3_GoodFlow_WithRFID()
    //{
    //  Assert.IsNotNull(api);
    //  Assert.IsNotNull(Conveyor);

    //  PLC_ItemID = 1;
    //  JobID = 15478;
    //  //Setup test
    //  DbProduct product = GetProduct(JobID);
    //  string RFIDTagValue = "3500003E8033400000001D2C";
    //  PalletItem = CreatePallet(RFIDTagValue, 1, JobID, PLC_ItemID);

    //  //Trigger process
    //  int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
    //  if (RunTask(AtPositionTaskID))
    //  {
    //    string PLCItemLineReference = lineReference + "." + PLC_ItemID;
    //    CheckPalletData(PalletItem.ID, RFIDTagValue, 10, system.LocationID, PLCItemLineReference);
    //    DbItem ProductItem = GetProductItem(PalletItem);
    //    if (ProductItem == null) Assert.Fail("Product Item was not created and could not be found on the pallet item.");
    //    int FinalDestID = GetProductCustomPropertyValue(product.ID, "CPS.PRODUCT.CP.DESTINATIONLOCATIONID").AsInt(-1);
    //    CheckProductData(ProductItem.ID, ProductItem.UniqueID, 11, PalletItem.ID, system.LocationID, PLCItemLineReference, FinalDestID);
    //    //CheckSetDestination
    //    CheckSetDestination(nextConv_PositionID);
    //    CheckMessageUserState(120, TaskUserState.COMPLETE);
    //  }
    //}

    #endregion Good Flows

    #region Bad Flows

    [Test]
    public void PalletizerConveyorBadFlow_0_IncorrectPLCItemID()
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
    public void PalletizerConveyorBadFlow_0_NoPalletItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        CheckSetDestination(ejectConv_PositionID);
        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void PalletizerConveyorBadFlow_3_NoPalletItem()
    {
      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      PLC_ItemID = 1;

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