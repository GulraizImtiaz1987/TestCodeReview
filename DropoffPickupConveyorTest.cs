using ETS.Core.Api.Models.Data;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections.Generic;

namespace ConveyorProcessLogicTest
{
  internal class DropoffPickupConveyorTest : TestMethod
  {
    const string server = "192.168.22.101";
    const string database = "RW_WCS_CORE";
    const string login = "edbApp";
    const string password = "Sqlapp!23";
    const int systemID = 179; //Intermediate Product Pickup (CONV_500)
    const int OnlyProductRowCount = 7;
    const int DefaultProductRowCount = 21;
    const int AllProductRowCount = 28;
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
      // Update custom property User Material Routing set as true
      SetSystemCustomPropertyValue(systemID, "CP.USEMATERIALROUTING", 1);
      InsertTestDataIntoTblProductRouting(altRoutingSystemID);
    }

    #region Good Flows

    [Test]
    public void DropoffPickupConveyorGoodFlow_0()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

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
    public void DropoffPickupConveyorGoodFlow_1_ProductAlreadyExistInTblProductRoutingWithExpiredOthers()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      // insert into tblProductRouting with product ProductID.
      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID, alternateDestinationSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        // All other products will be deleted as the timestamp is too old
        // Records with same Product ID will be updated 
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductAlreadyExistInTblProductRoutingWithOthersValidProduceDate()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      string updateLastProduceTime = "Update tblProductRouting Set LastProduceTimeStamp = GetDate()";
      api.Util.Db.ExecuteSql(updateLastProduceTime);

      // insert into tblProductRouting with product ProductID.
      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID, alternateDestinationSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductAlreadyExistInTblProductRoutingWithOthersValidProduceAndPickupDate()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      string updateLastProduceTime = "UPDATE tblProductRouting SET LastProduceTimeStamp = SYSDATETIMEOFFSET(), LastPickUpTimeStamp = SYSDATETIMEOFFSET()";
      api.Util.Db.ExecuteSql(updateLastProduceTime);

      // insert into tblProductRouting with product ProductID.
      InsertMaterialTestDataIntoTblProductRouting(ProductItem.ProductID, alternateDestinationSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(AllProductRowCount, tblProductRoutingRow);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductDoesNotExistInTblProductRoutingWithExpiredOthersUseEject()
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
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        int EjectionLaneSystemID = GetEjectionLaneSystemID();

        CheckTblProductRoutingAllocatedSystem(ProductItem.ProductID, EjectionLaneSystemID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductDoesNotExistInTblProductRoutingWithExpiredOthersUseAltSystem()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      // update use materialRouting
      SetProductCustomPropertyValue(1, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        CheckTblProductRoutingAllocatedSystem(ProductItem.ProductID, altRoutingSystemID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      SetProductCustomPropertyValue(0, ProductItem.ProductID, "CP.USEALTERNATIVEROUTEASDEFAULT");
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductDoesNotExistInTblProductRouting_UseSetup()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      InsertTestDataIntoTblProductRoutingSetup(JobID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        List<int> allocatedSystemList = new List<int>() { 169, 170 };
        CheckTblProductRoutingAllocatedSystems(ProductItem.ProductID, allocatedSystemList);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_ProductDoesNotExistInTblProductRouting_NoSetup()
    {
      PLC_ItemID = 1;
      JobID = 3825;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting(altRoutingSystemID, ProductItem.ProductID);
        Assert.AreEqual(OnlyProductRowCount, tblProductRoutingRow);

        int EjectionLaneSystemID = GetEjectionLaneSystemID();
        CheckTblProductRoutingAllocatedSystem(ProductItem.ProductID, EjectionLaneSystemID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_1_DontUseProductRouting()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      SetSystemCustomPropertyValue(systemID, "USEMATERIALROUTING", 0);
      Init(systemID, server, database, login, password);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, warehouseLocationID, PLCReference);

        // Nothing changed in tblProductRouting
        int tblProductRoutingRow = CheckTblProductRouting();
        Assert.AreEqual(DefaultProductRowCount, tblProductRoutingRow);

        int EjectionLaneSystemID = GetEjectionLaneSystemID();
        CheckTblProductRoutingAllocatedSystem(ProductItem.ProductID, EjectionLaneSystemID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the custom property changes
      SetSystemCustomPropertyValue(systemID, "USEMATERIALROUTING", 1);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_2()
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

    [Test]
    public void DropoffPickupConveyorGoodFlow_3_FinalDestinationNextSystem()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, nextLinkedSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_3_FinalDestinationNextSystem_NoProduct()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID, nextLinkedSystemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference, nextLinkedSystemID);

        CheckSetDestination(nextConv_PositionID);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_3_CreatePickupRequest()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID, systemID);

      int nextSystemID = nextLinkedSystemID;
      SetSystemCustomPropertyValue(systemID, "NEXTLINKEDCONVEYORSYSTEMID", -1);
      Init(systemID, server, database, login, password);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 3);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);
        CheckProductData(ProductItem.ID, ProductItem.UniqueID, productItemDefinitionID, PalletItem.ID, system.LocationID, PLCReference);

        // Check pickup task created
        DbLocation loc = GetLocationByID(system.LocationID);
        DbProduct product = GetProductByID(ProductItem.ProductID);
        CheckPalletOperationRequest(ProductItem.UniqueID, system.AltName, loc.AltName, product.Attribute01, product.Attribute02, 8);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.PASS);
      }

      // Revert the next linked system back to the original system
      SetSystemCustomPropertyValue(systemID, "NEXTLINKEDCONVEYORSYSTEMID", nextSystemID);
      Init(systemID, server, database, login, password);
    }

    [Test]
    public void DropoffPickupConveyorGoodFlow_4()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);
      ProductItem = CreateProduct(1, JobID, PLC_ItemID, PalletItem.ID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

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
    public void DropoffPickupConveyorBadFlow_0_NoPLCItemID()
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

    [Test]
    public void DropoffPickupConveyorBadFlow_0_NoPalletFound()
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
    public void DropoffPickupConveyorBadFlow_0_NoProductFound()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 0);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = lineReference + "." + PLC_ItemID;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, system.LocationID, PLCReference);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void DropoffPickupConveyorBadFlow_1_NoProductProductDoesNotExistInTblProductRoutingWithExpiredOthersUseEject()
    {
      PLC_ItemID = 1;
      JobID = 15478;

      PalletItem = CreatePallet("", 1, JobID, PLC_ItemID);

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        string PLCReference = string.Empty;
        CheckPalletData(PalletItem.ID, PalletItem.UniqueID, palletItemDefinitionID, warehouseLocationID, PLCReference);

        int tblProductRoutingRow = CheckTblProductRouting();
        Assert.AreEqual(tblProductRoutingRow, DefaultProductRowCount);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void DropoffPickupConveyorBadFlow_1_NoPalletProductDoesNotExistInTblProductRoutingWithExpiredOthersUseEject()
    {
      PLC_ItemID = 1;

      Assert.IsNotNull(api);
      Assert.IsNotNull(Conveyor);

      int AtPositionTaskID = SendAtPosition(PLC_ItemID, 1);
      if (RunTask(AtPositionTaskID))
      {
        int tblProductRoutingRow = CheckTblProductRouting();
        Assert.AreEqual(tblProductRoutingRow, DefaultProductRowCount);

        CheckMessageUserState(120, TaskUserState.COMPLETE);
        CheckMessagePassFail(120, TaskPassFail.FAIL);
      }
    }

    [Test]
    public void DropoffPickupConveyorBadFlow_3_NoPallet()
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

    #endregion Bad Flows
  }
}
