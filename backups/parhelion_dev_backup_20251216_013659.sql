--
-- PostgreSQL database dump
--

\restrict vPbXrNWUlquOeEJ5QzhfD8JnUNQKiT4WuaC9WX29tPKWN2DF69yiKlPnA6Wcagr

-- Dumped from database version 17.6 (Debian 17.6-2.pgdg13+1)
-- Dumped by pg_dump version 17.6 (Debian 17.6-2.pgdg13+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: CatalogItems; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."CatalogItems" (
    "Id" uuid NOT NULL,
    "Sku" character varying(50) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "BaseUom" character varying(20) NOT NULL,
    "DefaultWeightKg" numeric(10,3) NOT NULL,
    "DefaultWidthCm" numeric(10,2) NOT NULL,
    "DefaultHeightCm" numeric(10,2) NOT NULL,
    "DefaultLengthCm" numeric(10,2) NOT NULL,
    "RequiresRefrigeration" boolean NOT NULL,
    "IsHazardous" boolean NOT NULL,
    "IsFragile" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid,
    "TenantId" uuid NOT NULL
);


ALTER TABLE public."CatalogItems" OWNER TO "MetaCodeX";

--
-- Name: Clients; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Clients" (
    "Id" uuid NOT NULL,
    "CompanyName" character varying(200) NOT NULL,
    "TradeName" character varying(200),
    "ContactName" character varying(150) NOT NULL,
    "Email" character varying(150) NOT NULL,
    "Phone" character varying(30) NOT NULL,
    "TaxId" character varying(20),
    "LegalName" character varying(300),
    "BillingAddress" character varying(500),
    "ShippingAddress" character varying(500) NOT NULL,
    "PreferredProductTypes" character varying(300),
    "Priority" character varying(20) NOT NULL,
    "IsActive" boolean NOT NULL,
    "Notes" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Clients" OWNER TO "MetaCodeX";

--
-- Name: Drivers; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Drivers" (
    "Id" uuid NOT NULL,
    "EmployeeId" uuid NOT NULL,
    "LicenseNumber" character varying(50) NOT NULL,
    "DefaultTruckId" uuid,
    "CurrentTruckId" uuid,
    "Status" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid,
    "LicenseExpiration" timestamp with time zone,
    "LicenseType" character varying(10),
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Drivers" OWNER TO "MetaCodeX";

--
-- Name: Employees; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Employees" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Phone" character varying(20) NOT NULL,
    "Rfc" character varying(13),
    "Nss" character varying(11),
    "Curp" character varying(18),
    "EmergencyContact" character varying(200),
    "EmergencyPhone" character varying(20),
    "HireDate" timestamp with time zone,
    "ShiftId" uuid,
    "Department" character varying(50),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Employees" OWNER TO "MetaCodeX";

--
-- Name: FleetLogs; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."FleetLogs" (
    "Id" uuid NOT NULL,
    "DriverId" uuid NOT NULL,
    "OldTruckId" uuid,
    "NewTruckId" uuid NOT NULL,
    "Reason" integer NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."FleetLogs" OWNER TO "MetaCodeX";

--
-- Name: InventoryStocks; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."InventoryStocks" (
    "Id" uuid NOT NULL,
    "ZoneId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" numeric(18,4) NOT NULL,
    "QuantityReserved" numeric(18,4) NOT NULL,
    "BatchNumber" character varying(100),
    "ExpiryDate" timestamp with time zone,
    "LastCountDate" timestamp with time zone,
    "UnitCost" numeric(18,4),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid,
    "TenantId" uuid NOT NULL
);


ALTER TABLE public."InventoryStocks" OWNER TO "MetaCodeX";

--
-- Name: InventoryTransactions; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."InventoryTransactions" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "OriginZoneId" uuid,
    "DestinationZoneId" uuid,
    "Quantity" numeric(18,4) NOT NULL,
    "TransactionType" integer NOT NULL,
    "PerformedByUserId" uuid NOT NULL,
    "ShipmentId" uuid,
    "BatchNumber" character varying(100),
    "Remarks" character varying(500),
    "Timestamp" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid,
    "TenantId" uuid NOT NULL
);


ALTER TABLE public."InventoryTransactions" OWNER TO "MetaCodeX";

--
-- Name: Locations; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Locations" (
    "Id" uuid NOT NULL,
    "Code" character varying(10) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Type" integer NOT NULL,
    "FullAddress" character varying(500) NOT NULL,
    "CanReceive" boolean NOT NULL,
    "CanDispatch" boolean NOT NULL,
    "IsInternal" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid,
    "Latitude" numeric(9,6),
    "Longitude" numeric(9,6)
);


ALTER TABLE public."Locations" OWNER TO "MetaCodeX";

--
-- Name: NetworkLinks; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."NetworkLinks" (
    "Id" uuid NOT NULL,
    "OriginLocationId" uuid NOT NULL,
    "DestinationLocationId" uuid NOT NULL,
    "LinkType" integer NOT NULL,
    "TransitTime" interval NOT NULL,
    "IsBidirectional" boolean NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."NetworkLinks" OWNER TO "MetaCodeX";

--
-- Name: RefreshTokens; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TokenHash" character varying(256) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsRevoked" boolean DEFAULT false NOT NULL,
    "RevokedAt" timestamp with time zone,
    "RevokedReason" character varying(200),
    "CreatedFromIp" character varying(45),
    "UserAgent" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."RefreshTokens" OWNER TO "MetaCodeX";

--
-- Name: Roles; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Roles" (
    "Id" uuid NOT NULL,
    "Name" character varying(50) NOT NULL,
    "Description" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Roles" OWNER TO "MetaCodeX";

--
-- Name: RouteBlueprints; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."RouteBlueprints" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "TotalSteps" integer NOT NULL,
    "TotalTransitTime" interval NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."RouteBlueprints" OWNER TO "MetaCodeX";

--
-- Name: RouteSteps; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."RouteSteps" (
    "Id" uuid NOT NULL,
    "RouteBlueprintId" uuid NOT NULL,
    "LocationId" uuid NOT NULL,
    "StepOrder" integer NOT NULL,
    "StandardTransitTime" interval NOT NULL,
    "StepType" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."RouteSteps" OWNER TO "MetaCodeX";

--
-- Name: Shifts; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Shifts" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "StartTime" time without time zone NOT NULL,
    "EndTime" time without time zone NOT NULL,
    "DaysOfWeek" character varying(50) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Shifts" OWNER TO "MetaCodeX";

--
-- Name: ShipmentCheckpoints; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."ShipmentCheckpoints" (
    "Id" uuid NOT NULL,
    "ShipmentId" uuid NOT NULL,
    "LocationId" uuid,
    "StatusCode" integer NOT NULL,
    "Remarks" character varying(1000),
    "Timestamp" timestamp with time zone NOT NULL,
    "CreatedByUserId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "ActionType" character varying(50),
    "HandledByDriverId" uuid,
    "LoadedOntoTruckId" uuid,
    "NewCustodian" character varying(200),
    "PreviousCustodian" character varying(200),
    "HandledByWarehouseOperatorId" uuid,
    "LastModifiedByUserId" uuid,
    "Latitude" numeric(9,6),
    "Longitude" numeric(9,6)
);


ALTER TABLE public."ShipmentCheckpoints" OWNER TO "MetaCodeX";

--
-- Name: ShipmentDocuments; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."ShipmentDocuments" (
    "Id" uuid NOT NULL,
    "ShipmentId" uuid NOT NULL,
    "DocumentType" integer NOT NULL,
    "FileUrl" character varying(500) NOT NULL,
    "GeneratedBy" character varying(50) NOT NULL,
    "GeneratedAt" timestamp with time zone NOT NULL,
    "ExpiresAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."ShipmentDocuments" OWNER TO "MetaCodeX";

--
-- Name: ShipmentItems; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."ShipmentItems" (
    "Id" uuid NOT NULL,
    "ShipmentId" uuid NOT NULL,
    "Sku" character varying(50),
    "Description" character varying(500) NOT NULL,
    "PackagingType" integer NOT NULL,
    "Quantity" integer NOT NULL,
    "WeightKg" numeric(10,2) NOT NULL,
    "WidthCm" numeric(10,2) NOT NULL,
    "HeightCm" numeric(10,2) NOT NULL,
    "LengthCm" numeric(10,2) NOT NULL,
    "DeclaredValue" numeric(18,2) NOT NULL,
    "IsFragile" boolean NOT NULL,
    "IsHazardous" boolean NOT NULL,
    "RequiresRefrigeration" boolean NOT NULL,
    "StackingInstructions" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid,
    "ProductId" uuid
);


ALTER TABLE public."ShipmentItems" OWNER TO "MetaCodeX";

--
-- Name: Shipments; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Shipments" (
    "Id" uuid NOT NULL,
    "TrackingNumber" character varying(20) NOT NULL,
    "QrCodeData" character varying(100) NOT NULL,
    "OriginLocationId" uuid NOT NULL,
    "DestinationLocationId" uuid NOT NULL,
    "AssignedRouteId" uuid,
    "CurrentStepOrder" integer,
    "RecipientName" character varying(200) NOT NULL,
    "RecipientPhone" character varying(20),
    "TotalWeightKg" numeric(10,2) NOT NULL,
    "TotalVolumeM3" numeric(10,3) NOT NULL,
    "DeclaredValue" numeric(18,2),
    "SatMerchandiseCode" character varying(20),
    "DeliveryInstructions" character varying(1000),
    "RecipientSignatureUrl" character varying(500),
    "Priority" integer NOT NULL,
    "Status" integer NOT NULL,
    "TruckId" uuid,
    "DriverId" uuid,
    "WasQrScanned" boolean NOT NULL,
    "IsDelayed" boolean NOT NULL,
    "ScheduledDeparture" timestamp with time zone,
    "PickupWindowStart" timestamp with time zone,
    "PickupWindowEnd" timestamp with time zone,
    "EstimatedArrival" timestamp with time zone,
    "AssignedAt" timestamp with time zone,
    "DeliveredAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "RecipientClientId" uuid,
    "SenderId" uuid,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Shipments" OWNER TO "MetaCodeX";

--
-- Name: Tenants; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Tenants" (
    "Id" uuid NOT NULL,
    "CompanyName" character varying(200) NOT NULL,
    "ContactEmail" character varying(256) NOT NULL,
    "FleetSize" integer NOT NULL,
    "DriverCount" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Tenants" OWNER TO "MetaCodeX";

--
-- Name: Trucks; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Trucks" (
    "Id" uuid NOT NULL,
    "Plate" character varying(20) NOT NULL,
    "Model" character varying(100) NOT NULL,
    "Type" integer NOT NULL,
    "MaxCapacityKg" numeric(10,2) NOT NULL,
    "MaxVolumeM3" numeric(10,2) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "Color" text,
    "CurrentOdometerKm" numeric,
    "EngineNumber" text,
    "InsuranceExpiration" timestamp with time zone,
    "InsurancePolicy" text,
    "LastMaintenanceDate" timestamp with time zone,
    "NextMaintenanceDate" timestamp with time zone,
    "VerificationExpiration" timestamp with time zone,
    "VerificationNumber" text,
    "Vin" text,
    "Year" integer,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Trucks" OWNER TO "MetaCodeX";

--
-- Name: Users; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PasswordHash" character varying(500) NOT NULL,
    "FullName" character varying(200) NOT NULL,
    "RoleId" uuid NOT NULL,
    "IsDemoUser" boolean NOT NULL,
    "UsesArgon2" boolean NOT NULL,
    "LastLogin" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "TenantId" uuid NOT NULL,
    "IsSuperAdmin" boolean DEFAULT false NOT NULL,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."Users" OWNER TO "MetaCodeX";

--
-- Name: WarehouseOperators; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."WarehouseOperators" (
    "Id" uuid NOT NULL,
    "EmployeeId" uuid NOT NULL,
    "AssignedLocationId" uuid NOT NULL,
    "PrimaryZoneId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."WarehouseOperators" OWNER TO "MetaCodeX";

--
-- Name: WarehouseZones; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."WarehouseZones" (
    "Id" uuid NOT NULL,
    "LocationId" uuid NOT NULL,
    "Code" character varying(20) NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Type" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedByUserId" uuid,
    "LastModifiedByUserId" uuid
);


ALTER TABLE public."WarehouseZones" OWNER TO "MetaCodeX";

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: MetaCodeX
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO "MetaCodeX";

--
-- Data for Name: CatalogItems; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."CatalogItems" ("Id", "Sku", "Name", "Description", "BaseUom", "DefaultWeightKg", "DefaultWidthCm", "DefaultHeightCm", "DefaultLengthCm", "RequiresRefrigeration", "IsHazardous", "IsFragile", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId", "TenantId") FROM stdin;
\.


--
-- Data for Name: Clients; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Clients" ("Id", "CompanyName", "TradeName", "ContactName", "Email", "Phone", "TaxId", "LegalName", "BillingAddress", "ShippingAddress", "PreferredProductTypes", "Priority", "IsActive", "Notes", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: Drivers; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Drivers" ("Id", "EmployeeId", "LicenseNumber", "DefaultTruckId", "CurrentTruckId", "Status", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "LicenseExpiration", "LicenseType", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: Employees; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Employees" ("Id", "UserId", "Phone", "Rfc", "Nss", "Curp", "EmergencyContact", "EmergencyPhone", "HireDate", "ShiftId", "Department", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: FleetLogs; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."FleetLogs" ("Id", "DriverId", "OldTruckId", "NewTruckId", "Reason", "Timestamp", "CreatedByUserId", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: InventoryStocks; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."InventoryStocks" ("Id", "ZoneId", "ProductId", "Quantity", "QuantityReserved", "BatchNumber", "ExpiryDate", "LastCountDate", "UnitCost", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId", "TenantId") FROM stdin;
\.


--
-- Data for Name: InventoryTransactions; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."InventoryTransactions" ("Id", "ProductId", "OriginZoneId", "DestinationZoneId", "Quantity", "TransactionType", "PerformedByUserId", "ShipmentId", "BatchNumber", "Remarks", "Timestamp", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId", "TenantId") FROM stdin;
\.


--
-- Data for Name: Locations; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Locations" ("Id", "Code", "Name", "Type", "FullAddress", "CanReceive", "CanDispatch", "IsInternal", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId", "Latitude", "Longitude") FROM stdin;
\.


--
-- Data for Name: NetworkLinks; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."NetworkLinks" ("Id", "OriginLocationId", "DestinationLocationId", "LinkType", "TransitTime", "IsBidirectional", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: RefreshTokens; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."RefreshTokens" ("Id", "UserId", "TokenHash", "ExpiresAt", "IsRevoked", "RevokedAt", "RevokedReason", "CreatedFromIp", "UserAgent", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
ef919193-8cb7-4f60-bc7c-6d240983b1eb	00000000-0000-0000-0000-000000000001	b3KSGCViUaxxaMF2duWSAltYtUxSrBSOjkTq+8axLuc=	2025-12-22 01:45:20.060723+00	f	\N	\N	127.0.0.1	node	2025-12-15 01:45:20.231994+00	\N	f	\N	\N	\N
4fb00dab-0bd4-4419-a8ee-3b610489e0a0	00000000-0000-0000-0000-000000000001	36WZ5Vm29xcS3wkZsZvLB7dT4BKAEPDLEgfM/oISGJY=	2025-12-22 01:46:18.594664+00	f	\N	\N	127.0.0.1	node	2025-12-15 01:46:18.60054+00	\N	f	\N	\N	\N
e6502b63-f8e3-41d2-9b54-c88f5fb787a6	00000000-0000-0000-0000-000000000001	ZJHwla8b3fgW18CHviD2FbSEkQEMcwixjrHUzJCtQM0=	2025-12-22 01:50:49.93796+00	f	\N	\N	127.0.0.1	node	2025-12-15 01:50:49.940207+00	\N	f	\N	\N	\N
5ae661fa-f1ef-4b3d-8f2f-fadab41009fa	00000000-0000-0000-0000-000000000001	t//Mr7dPMKQxrb08CwKi01+m7V5KSiN1yUZZBrwKKOw=	2025-12-22 04:09:00.00483+00	f	\N	\N	127.0.0.1	node	2025-12-15 04:09:00.234369+00	\N	f	\N	\N	\N
\.


--
-- Data for Name: Roles; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Roles" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
11111111-1111-1111-1111-111111111111	Admin	Gerente de Tráfico - Acceso total al sistema	2025-12-13 00:20:55.508718+00	\N	f	\N	\N	\N
22222222-2222-2222-2222-222222222222	Driver	Chofer - Solo ve sus envíos asignados	2025-12-13 00:20:55.508718+00	\N	f	\N	\N	\N
33333333-3333-3333-3333-333333333333	DemoUser	Usuario de demostración temporal (24-48h)	2025-12-13 00:20:55.508718+00	\N	f	\N	\N	\N
44444444-4444-4444-4444-444444444444	Warehouse	Almacenista - Gestiona carga y descarga de camiones	2025-12-13 00:20:55.508718+00	\N	f	\N	\N	\N
55555555-5555-5555-5555-555555555555	SystemAdmin	Super Admin - Gestiona tenants y administradores (v0.4.3)	2025-12-14 02:04:53.167361+00	\N	f	\N	\N	\N
00000000-0000-0000-0000-000000000001	SuperAdmin	Super Administrator with full system access	2025-12-15 01:44:01.669271+00	\N	f	\N	\N	\N
\.


--
-- Data for Name: RouteBlueprints; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."RouteBlueprints" ("Id", "Name", "Description", "TotalSteps", "TotalTransitTime", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: RouteSteps; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."RouteSteps" ("Id", "RouteBlueprintId", "LocationId", "StepOrder", "StandardTransitTime", "StepType", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: Shifts; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Shifts" ("Id", "Name", "StartTime", "EndTime", "DaysOfWeek", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: ShipmentCheckpoints; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."ShipmentCheckpoints" ("Id", "ShipmentId", "LocationId", "StatusCode", "Remarks", "Timestamp", "CreatedByUserId", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "ActionType", "HandledByDriverId", "LoadedOntoTruckId", "NewCustodian", "PreviousCustodian", "HandledByWarehouseOperatorId", "LastModifiedByUserId", "Latitude", "Longitude") FROM stdin;
\.


--
-- Data for Name: ShipmentDocuments; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."ShipmentDocuments" ("Id", "ShipmentId", "DocumentType", "FileUrl", "GeneratedBy", "GeneratedAt", "ExpiresAt", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: ShipmentItems; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."ShipmentItems" ("Id", "ShipmentId", "Sku", "Description", "PackagingType", "Quantity", "WeightKg", "WidthCm", "HeightCm", "LengthCm", "DeclaredValue", "IsFragile", "IsHazardous", "RequiresRefrigeration", "StackingInstructions", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId", "ProductId") FROM stdin;
\.


--
-- Data for Name: Shipments; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Shipments" ("Id", "TrackingNumber", "QrCodeData", "OriginLocationId", "DestinationLocationId", "AssignedRouteId", "CurrentStepOrder", "RecipientName", "RecipientPhone", "TotalWeightKg", "TotalVolumeM3", "DeclaredValue", "SatMerchandiseCode", "DeliveryInstructions", "RecipientSignatureUrl", "Priority", "Status", "TruckId", "DriverId", "WasQrScanned", "IsDelayed", "ScheduledDeparture", "PickupWindowStart", "PickupWindowEnd", "EstimatedArrival", "AssignedAt", "DeliveredAt", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "RecipientClientId", "SenderId", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: Tenants; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Tenants" ("Id", "CompanyName", "ContactEmail", "FleetSize", "DriverCount", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
00000000-0000-0000-0000-000000000001	Parhelion Logistics	admin@parhelion.com	0	0	t	2025-12-15 01:44:39.806809+00	\N	f	\N	\N	\N
\.


--
-- Data for Name: Trucks; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Trucks" ("Id", "Plate", "Model", "Type", "MaxCapacityKg", "MaxVolumeM3", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "Color", "CurrentOdometerKm", "EngineNumber", "InsuranceExpiration", "InsurancePolicy", "LastMaintenanceDate", "NextMaintenanceDate", "VerificationExpiration", "VerificationNumber", "Vin", "Year", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."Users" ("Id", "Email", "PasswordHash", "FullName", "RoleId", "IsDemoUser", "UsesArgon2", "LastLogin", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "TenantId", "IsSuperAdmin", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
00000000-0000-0000-0000-000000000001	metacodex@parhelion.com	$2b$14$biNEvC.Y.mAhfgWvgM5SyugH3xOIEI2oDAuqKpvcktwy9KsBvQCxK	MetaCodeX CEO	00000000-0000-0000-0000-000000000001	f	t	2025-12-15 04:08:59.962106+00	t	2025-12-15 01:44:59.445268+00	2025-12-15 04:09:00.234369+00	f	\N	00000000-0000-0000-0000-000000000001	t	\N	\N
\.


--
-- Data for Name: WarehouseOperators; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."WarehouseOperators" ("Id", "EmployeeId", "AssignedLocationId", "PrimaryZoneId", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: WarehouseZones; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."WarehouseZones" ("Id", "LocationId", "Code", "Name", "Type", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted", "DeletedAt", "CreatedByUserId", "LastModifiedByUserId") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: MetaCodeX
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251213001913_InitialCreate	8.0.10
20251213030538_AddAuthAndClients	8.0.10
20251213194319_AddEmployeeLayerV043	8.0.10
20251214153448_WmsEnhancement044	8.0.10
\.


--
-- Name: CatalogItems PK_CatalogItems; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."CatalogItems"
    ADD CONSTRAINT "PK_CatalogItems" PRIMARY KEY ("Id");


--
-- Name: Clients PK_Clients; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Clients"
    ADD CONSTRAINT "PK_Clients" PRIMARY KEY ("Id");


--
-- Name: Drivers PK_Drivers; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Drivers"
    ADD CONSTRAINT "PK_Drivers" PRIMARY KEY ("Id");


--
-- Name: Employees PK_Employees; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Employees"
    ADD CONSTRAINT "PK_Employees" PRIMARY KEY ("Id");


--
-- Name: FleetLogs PK_FleetLogs; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "PK_FleetLogs" PRIMARY KEY ("Id");


--
-- Name: InventoryStocks PK_InventoryStocks; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryStocks"
    ADD CONSTRAINT "PK_InventoryStocks" PRIMARY KEY ("Id");


--
-- Name: InventoryTransactions PK_InventoryTransactions; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "PK_InventoryTransactions" PRIMARY KEY ("Id");


--
-- Name: Locations PK_Locations; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Locations"
    ADD CONSTRAINT "PK_Locations" PRIMARY KEY ("Id");


--
-- Name: NetworkLinks PK_NetworkLinks; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."NetworkLinks"
    ADD CONSTRAINT "PK_NetworkLinks" PRIMARY KEY ("Id");


--
-- Name: RefreshTokens PK_RefreshTokens; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id");


--
-- Name: Roles PK_Roles; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "PK_Roles" PRIMARY KEY ("Id");


--
-- Name: RouteBlueprints PK_RouteBlueprints; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RouteBlueprints"
    ADD CONSTRAINT "PK_RouteBlueprints" PRIMARY KEY ("Id");


--
-- Name: RouteSteps PK_RouteSteps; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RouteSteps"
    ADD CONSTRAINT "PK_RouteSteps" PRIMARY KEY ("Id");


--
-- Name: Shifts PK_Shifts; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shifts"
    ADD CONSTRAINT "PK_Shifts" PRIMARY KEY ("Id");


--
-- Name: ShipmentCheckpoints PK_ShipmentCheckpoints; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "PK_ShipmentCheckpoints" PRIMARY KEY ("Id");


--
-- Name: ShipmentDocuments PK_ShipmentDocuments; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentDocuments"
    ADD CONSTRAINT "PK_ShipmentDocuments" PRIMARY KEY ("Id");


--
-- Name: ShipmentItems PK_ShipmentItems; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentItems"
    ADD CONSTRAINT "PK_ShipmentItems" PRIMARY KEY ("Id");


--
-- Name: Shipments PK_Shipments; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "PK_Shipments" PRIMARY KEY ("Id");


--
-- Name: Tenants PK_Tenants; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Tenants"
    ADD CONSTRAINT "PK_Tenants" PRIMARY KEY ("Id");


--
-- Name: Trucks PK_Trucks; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Trucks"
    ADD CONSTRAINT "PK_Trucks" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: WarehouseOperators PK_WarehouseOperators; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseOperators"
    ADD CONSTRAINT "PK_WarehouseOperators" PRIMARY KEY ("Id");


--
-- Name: WarehouseZones PK_WarehouseZones; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseZones"
    ADD CONSTRAINT "PK_WarehouseZones" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_CatalogItems_TenantId_Sku; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_CatalogItems_TenantId_Sku" ON public."CatalogItems" USING btree ("TenantId", "Sku");


--
-- Name: IX_Clients_Email; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Clients_Email" ON public."Clients" USING btree ("Email");


--
-- Name: IX_Clients_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Clients_TenantId" ON public."Clients" USING btree ("TenantId");


--
-- Name: IX_Clients_TenantId_CompanyName; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Clients_TenantId_CompanyName" ON public."Clients" USING btree ("TenantId", "CompanyName");


--
-- Name: IX_Drivers_CurrentTruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Drivers_CurrentTruckId" ON public."Drivers" USING btree ("CurrentTruckId");


--
-- Name: IX_Drivers_DefaultTruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Drivers_DefaultTruckId" ON public."Drivers" USING btree ("DefaultTruckId");


--
-- Name: IX_Drivers_EmployeeId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Drivers_EmployeeId" ON public."Drivers" USING btree ("EmployeeId");


--
-- Name: IX_Drivers_Status; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Drivers_Status" ON public."Drivers" USING btree ("Status");


--
-- Name: IX_Drivers_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Drivers_TenantId" ON public."Drivers" USING btree ("TenantId");


--
-- Name: IX_Employees_ShiftId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Employees_ShiftId" ON public."Employees" USING btree ("ShiftId");


--
-- Name: IX_Employees_TenantId_Department; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Employees_TenantId_Department" ON public."Employees" USING btree ("TenantId", "Department");


--
-- Name: IX_Employees_UserId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Employees_UserId" ON public."Employees" USING btree ("UserId");


--
-- Name: IX_FleetLogs_CreatedByUserId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_FleetLogs_CreatedByUserId" ON public."FleetLogs" USING btree ("CreatedByUserId");


--
-- Name: IX_FleetLogs_DriverId_Timestamp; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_FleetLogs_DriverId_Timestamp" ON public."FleetLogs" USING btree ("DriverId", "Timestamp");


--
-- Name: IX_FleetLogs_NewTruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_FleetLogs_NewTruckId" ON public."FleetLogs" USING btree ("NewTruckId");


--
-- Name: IX_FleetLogs_OldTruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_FleetLogs_OldTruckId" ON public."FleetLogs" USING btree ("OldTruckId");


--
-- Name: IX_FleetLogs_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_FleetLogs_TenantId" ON public."FleetLogs" USING btree ("TenantId");


--
-- Name: IX_InventoryStocks_ProductId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryStocks_ProductId" ON public."InventoryStocks" USING btree ("ProductId");


--
-- Name: IX_InventoryStocks_TenantId_ExpiryDate; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryStocks_TenantId_ExpiryDate" ON public."InventoryStocks" USING btree ("TenantId", "ExpiryDate") WHERE ("ExpiryDate" IS NOT NULL);


--
-- Name: IX_InventoryStocks_TenantId_ProductId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryStocks_TenantId_ProductId" ON public."InventoryStocks" USING btree ("TenantId", "ProductId");


--
-- Name: IX_InventoryStocks_ZoneId_ProductId_BatchNumber; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_InventoryStocks_ZoneId_ProductId_BatchNumber" ON public."InventoryStocks" USING btree ("ZoneId", "ProductId", "BatchNumber");


--
-- Name: IX_InventoryTransactions_DestinationZoneId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_DestinationZoneId" ON public."InventoryTransactions" USING btree ("DestinationZoneId");


--
-- Name: IX_InventoryTransactions_OriginZoneId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_OriginZoneId" ON public."InventoryTransactions" USING btree ("OriginZoneId");


--
-- Name: IX_InventoryTransactions_PerformedByUserId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_PerformedByUserId" ON public."InventoryTransactions" USING btree ("PerformedByUserId");


--
-- Name: IX_InventoryTransactions_ProductId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_ProductId" ON public."InventoryTransactions" USING btree ("ProductId");


--
-- Name: IX_InventoryTransactions_ShipmentId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_ShipmentId" ON public."InventoryTransactions" USING btree ("ShipmentId") WHERE ("ShipmentId" IS NOT NULL);


--
-- Name: IX_InventoryTransactions_TenantId_ProductId_Timestamp; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_TenantId_ProductId_Timestamp" ON public."InventoryTransactions" USING btree ("TenantId", "ProductId", "Timestamp");


--
-- Name: IX_InventoryTransactions_TenantId_Timestamp; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_InventoryTransactions_TenantId_Timestamp" ON public."InventoryTransactions" USING btree ("TenantId", "Timestamp");


--
-- Name: IX_Locations_TenantId_Code; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Locations_TenantId_Code" ON public."Locations" USING btree ("TenantId", "Code");


--
-- Name: IX_Locations_TenantId_Type; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Locations_TenantId_Type" ON public."Locations" USING btree ("TenantId", "Type");


--
-- Name: IX_NetworkLinks_DestinationLocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_NetworkLinks_DestinationLocationId" ON public."NetworkLinks" USING btree ("DestinationLocationId");


--
-- Name: IX_NetworkLinks_OriginLocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_NetworkLinks_OriginLocationId" ON public."NetworkLinks" USING btree ("OriginLocationId");


--
-- Name: IX_NetworkLinks_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_NetworkLinks_TenantId" ON public."NetworkLinks" USING btree ("TenantId");


--
-- Name: IX_RefreshTokens_ExpiresAt; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RefreshTokens_ExpiresAt" ON public."RefreshTokens" USING btree ("ExpiresAt");


--
-- Name: IX_RefreshTokens_TokenHash; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RefreshTokens_TokenHash" ON public."RefreshTokens" USING btree ("TokenHash");


--
-- Name: IX_RefreshTokens_UserId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RefreshTokens_UserId" ON public."RefreshTokens" USING btree ("UserId");


--
-- Name: IX_Roles_Name; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Roles_Name" ON public."Roles" USING btree ("Name");


--
-- Name: IX_RouteBlueprints_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RouteBlueprints_TenantId" ON public."RouteBlueprints" USING btree ("TenantId");


--
-- Name: IX_RouteSteps_LocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RouteSteps_LocationId" ON public."RouteSteps" USING btree ("LocationId");


--
-- Name: IX_RouteSteps_RouteBlueprintId_StepOrder; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_RouteSteps_RouteBlueprintId_StepOrder" ON public."RouteSteps" USING btree ("RouteBlueprintId", "StepOrder");


--
-- Name: IX_Shifts_TenantId_IsActive; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shifts_TenantId_IsActive" ON public."Shifts" USING btree ("TenantId", "IsActive");


--
-- Name: IX_ShipmentCheckpoints_CreatedByUserId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_CreatedByUserId" ON public."ShipmentCheckpoints" USING btree ("CreatedByUserId");


--
-- Name: IX_ShipmentCheckpoints_HandledByDriverId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_HandledByDriverId" ON public."ShipmentCheckpoints" USING btree ("HandledByDriverId");


--
-- Name: IX_ShipmentCheckpoints_HandledByWarehouseOperatorId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_HandledByWarehouseOperatorId" ON public."ShipmentCheckpoints" USING btree ("HandledByWarehouseOperatorId");


--
-- Name: IX_ShipmentCheckpoints_LoadedOntoTruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_LoadedOntoTruckId" ON public."ShipmentCheckpoints" USING btree ("LoadedOntoTruckId");


--
-- Name: IX_ShipmentCheckpoints_LocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_LocationId" ON public."ShipmentCheckpoints" USING btree ("LocationId");


--
-- Name: IX_ShipmentCheckpoints_ShipmentId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_ShipmentId" ON public."ShipmentCheckpoints" USING btree ("ShipmentId");


--
-- Name: IX_ShipmentCheckpoints_Timestamp; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentCheckpoints_Timestamp" ON public."ShipmentCheckpoints" USING btree ("Timestamp");


--
-- Name: IX_ShipmentDocuments_ShipmentId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentDocuments_ShipmentId" ON public."ShipmentDocuments" USING btree ("ShipmentId");


--
-- Name: IX_ShipmentItems_ProductId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentItems_ProductId" ON public."ShipmentItems" USING btree ("ProductId");


--
-- Name: IX_ShipmentItems_ShipmentId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_ShipmentItems_ShipmentId" ON public."ShipmentItems" USING btree ("ShipmentId");


--
-- Name: IX_Shipments_AssignedRouteId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_AssignedRouteId" ON public."Shipments" USING btree ("AssignedRouteId");


--
-- Name: IX_Shipments_DestinationLocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_DestinationLocationId" ON public."Shipments" USING btree ("DestinationLocationId");


--
-- Name: IX_Shipments_DriverId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_DriverId" ON public."Shipments" USING btree ("DriverId");


--
-- Name: IX_Shipments_OriginLocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_OriginLocationId" ON public."Shipments" USING btree ("OriginLocationId");


--
-- Name: IX_Shipments_RecipientClientId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_RecipientClientId" ON public."Shipments" USING btree ("RecipientClientId");


--
-- Name: IX_Shipments_SenderId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_SenderId" ON public."Shipments" USING btree ("SenderId");


--
-- Name: IX_Shipments_TenantId_CreatedAt; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_TenantId_CreatedAt" ON public."Shipments" USING btree ("TenantId", "CreatedAt");


--
-- Name: IX_Shipments_TenantId_IsDelayed; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_TenantId_IsDelayed" ON public."Shipments" USING btree ("TenantId", "IsDelayed") WHERE ("IsDelayed" = true);


--
-- Name: IX_Shipments_TenantId_Status; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_TenantId_Status" ON public."Shipments" USING btree ("TenantId", "Status");


--
-- Name: IX_Shipments_TrackingNumber; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Shipments_TrackingNumber" ON public."Shipments" USING btree ("TrackingNumber");


--
-- Name: IX_Shipments_TruckId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Shipments_TruckId" ON public."Shipments" USING btree ("TruckId");


--
-- Name: IX_Tenants_IsActive; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Tenants_IsActive" ON public."Tenants" USING btree ("IsActive");


--
-- Name: IX_Trucks_TenantId_Plate; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Trucks_TenantId_Plate" ON public."Trucks" USING btree ("TenantId", "Plate");


--
-- Name: IX_Trucks_TenantId_Type; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Trucks_TenantId_Type" ON public."Trucks" USING btree ("TenantId", "Type");


--
-- Name: IX_Users_Email; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_Users_Email" ON public."Users" USING btree ("Email");


--
-- Name: IX_Users_RoleId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Users_RoleId" ON public."Users" USING btree ("RoleId");


--
-- Name: IX_Users_TenantId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_Users_TenantId" ON public."Users" USING btree ("TenantId");


--
-- Name: IX_WarehouseOperators_AssignedLocationId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_WarehouseOperators_AssignedLocationId" ON public."WarehouseOperators" USING btree ("AssignedLocationId");


--
-- Name: IX_WarehouseOperators_EmployeeId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_WarehouseOperators_EmployeeId" ON public."WarehouseOperators" USING btree ("EmployeeId");


--
-- Name: IX_WarehouseOperators_PrimaryZoneId; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE INDEX "IX_WarehouseOperators_PrimaryZoneId" ON public."WarehouseOperators" USING btree ("PrimaryZoneId");


--
-- Name: IX_WarehouseZones_LocationId_Code; Type: INDEX; Schema: public; Owner: MetaCodeX
--

CREATE UNIQUE INDEX "IX_WarehouseZones_LocationId_Code" ON public."WarehouseZones" USING btree ("LocationId", "Code");


--
-- Name: CatalogItems FK_CatalogItems_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."CatalogItems"
    ADD CONSTRAINT "FK_CatalogItems_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: Clients FK_Clients_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Clients"
    ADD CONSTRAINT "FK_Clients_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: Drivers FK_Drivers_Employees_EmployeeId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Drivers"
    ADD CONSTRAINT "FK_Drivers_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES public."Employees"("Id") ON DELETE CASCADE;


--
-- Name: Drivers FK_Drivers_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Drivers"
    ADD CONSTRAINT "FK_Drivers_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id");


--
-- Name: Drivers FK_Drivers_Trucks_CurrentTruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Drivers"
    ADD CONSTRAINT "FK_Drivers_Trucks_CurrentTruckId" FOREIGN KEY ("CurrentTruckId") REFERENCES public."Trucks"("Id") ON DELETE SET NULL;


--
-- Name: Drivers FK_Drivers_Trucks_DefaultTruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Drivers"
    ADD CONSTRAINT "FK_Drivers_Trucks_DefaultTruckId" FOREIGN KEY ("DefaultTruckId") REFERENCES public."Trucks"("Id") ON DELETE SET NULL;


--
-- Name: Employees FK_Employees_Shifts_ShiftId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Employees"
    ADD CONSTRAINT "FK_Employees_Shifts_ShiftId" FOREIGN KEY ("ShiftId") REFERENCES public."Shifts"("Id") ON DELETE SET NULL;


--
-- Name: Employees FK_Employees_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Employees"
    ADD CONSTRAINT "FK_Employees_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: Employees FK_Employees_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Employees"
    ADD CONSTRAINT "FK_Employees_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: FleetLogs FK_FleetLogs_Drivers_DriverId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "FK_FleetLogs_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES public."Drivers"("Id") ON DELETE RESTRICT;


--
-- Name: FleetLogs FK_FleetLogs_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "FK_FleetLogs_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: FleetLogs FK_FleetLogs_Trucks_NewTruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "FK_FleetLogs_Trucks_NewTruckId" FOREIGN KEY ("NewTruckId") REFERENCES public."Trucks"("Id") ON DELETE RESTRICT;


--
-- Name: FleetLogs FK_FleetLogs_Trucks_OldTruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "FK_FleetLogs_Trucks_OldTruckId" FOREIGN KEY ("OldTruckId") REFERENCES public."Trucks"("Id") ON DELETE SET NULL;


--
-- Name: FleetLogs FK_FleetLogs_Users_CreatedByUserId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."FleetLogs"
    ADD CONSTRAINT "FK_FleetLogs_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryStocks FK_InventoryStocks_CatalogItems_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryStocks"
    ADD CONSTRAINT "FK_InventoryStocks_CatalogItems_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."CatalogItems"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryStocks FK_InventoryStocks_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryStocks"
    ADD CONSTRAINT "FK_InventoryStocks_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryStocks FK_InventoryStocks_WarehouseZones_ZoneId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryStocks"
    ADD CONSTRAINT "FK_InventoryStocks_WarehouseZones_ZoneId" FOREIGN KEY ("ZoneId") REFERENCES public."WarehouseZones"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryTransactions FK_InventoryTransactions_CatalogItems_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_CatalogItems_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."CatalogItems"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryTransactions FK_InventoryTransactions_Shipments_ShipmentId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_Shipments_ShipmentId" FOREIGN KEY ("ShipmentId") REFERENCES public."Shipments"("Id") ON DELETE SET NULL;


--
-- Name: InventoryTransactions FK_InventoryTransactions_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryTransactions FK_InventoryTransactions_Users_PerformedByUserId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_Users_PerformedByUserId" FOREIGN KEY ("PerformedByUserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: InventoryTransactions FK_InventoryTransactions_WarehouseZones_DestinationZoneId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_WarehouseZones_DestinationZoneId" FOREIGN KEY ("DestinationZoneId") REFERENCES public."WarehouseZones"("Id") ON DELETE SET NULL;


--
-- Name: InventoryTransactions FK_InventoryTransactions_WarehouseZones_OriginZoneId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."InventoryTransactions"
    ADD CONSTRAINT "FK_InventoryTransactions_WarehouseZones_OriginZoneId" FOREIGN KEY ("OriginZoneId") REFERENCES public."WarehouseZones"("Id") ON DELETE SET NULL;


--
-- Name: Locations FK_Locations_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Locations"
    ADD CONSTRAINT "FK_Locations_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: NetworkLinks FK_NetworkLinks_Locations_DestinationLocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."NetworkLinks"
    ADD CONSTRAINT "FK_NetworkLinks_Locations_DestinationLocationId" FOREIGN KEY ("DestinationLocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: NetworkLinks FK_NetworkLinks_Locations_OriginLocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."NetworkLinks"
    ADD CONSTRAINT "FK_NetworkLinks_Locations_OriginLocationId" FOREIGN KEY ("OriginLocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: NetworkLinks FK_NetworkLinks_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."NetworkLinks"
    ADD CONSTRAINT "FK_NetworkLinks_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: RefreshTokens FK_RefreshTokens_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RefreshTokens"
    ADD CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: RouteBlueprints FK_RouteBlueprints_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RouteBlueprints"
    ADD CONSTRAINT "FK_RouteBlueprints_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: RouteSteps FK_RouteSteps_Locations_LocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RouteSteps"
    ADD CONSTRAINT "FK_RouteSteps_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: RouteSteps FK_RouteSteps_RouteBlueprints_RouteBlueprintId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."RouteSteps"
    ADD CONSTRAINT "FK_RouteSteps_RouteBlueprints_RouteBlueprintId" FOREIGN KEY ("RouteBlueprintId") REFERENCES public."RouteBlueprints"("Id") ON DELETE CASCADE;


--
-- Name: Shifts FK_Shifts_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shifts"
    ADD CONSTRAINT "FK_Shifts_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_Drivers_HandledByDriverId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_Drivers_HandledByDriverId" FOREIGN KEY ("HandledByDriverId") REFERENCES public."Drivers"("Id") ON DELETE SET NULL;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_Locations_LocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES public."Locations"("Id") ON DELETE SET NULL;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_Shipments_ShipmentId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_Shipments_ShipmentId" FOREIGN KEY ("ShipmentId") REFERENCES public."Shipments"("Id") ON DELETE CASCADE;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_Trucks_LoadedOntoTruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_Trucks_LoadedOntoTruckId" FOREIGN KEY ("LoadedOntoTruckId") REFERENCES public."Trucks"("Id") ON DELETE SET NULL;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_Users_CreatedByUserId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_Users_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: ShipmentCheckpoints FK_ShipmentCheckpoints_WarehouseOperators_HandledByWarehouseOp~; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentCheckpoints"
    ADD CONSTRAINT "FK_ShipmentCheckpoints_WarehouseOperators_HandledByWarehouseOp~" FOREIGN KEY ("HandledByWarehouseOperatorId") REFERENCES public."WarehouseOperators"("Id");


--
-- Name: ShipmentDocuments FK_ShipmentDocuments_Shipments_ShipmentId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentDocuments"
    ADD CONSTRAINT "FK_ShipmentDocuments_Shipments_ShipmentId" FOREIGN KEY ("ShipmentId") REFERENCES public."Shipments"("Id") ON DELETE CASCADE;


--
-- Name: ShipmentItems FK_ShipmentItems_CatalogItems_ProductId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentItems"
    ADD CONSTRAINT "FK_ShipmentItems_CatalogItems_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."CatalogItems"("Id") ON DELETE SET NULL;


--
-- Name: ShipmentItems FK_ShipmentItems_Shipments_ShipmentId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."ShipmentItems"
    ADD CONSTRAINT "FK_ShipmentItems_Shipments_ShipmentId" FOREIGN KEY ("ShipmentId") REFERENCES public."Shipments"("Id") ON DELETE CASCADE;


--
-- Name: Shipments FK_Shipments_Clients_RecipientClientId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Clients_RecipientClientId" FOREIGN KEY ("RecipientClientId") REFERENCES public."Clients"("Id") ON DELETE SET NULL;


--
-- Name: Shipments FK_Shipments_Clients_SenderId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Clients_SenderId" FOREIGN KEY ("SenderId") REFERENCES public."Clients"("Id") ON DELETE SET NULL;


--
-- Name: Shipments FK_Shipments_Drivers_DriverId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Drivers_DriverId" FOREIGN KEY ("DriverId") REFERENCES public."Drivers"("Id") ON DELETE SET NULL;


--
-- Name: Shipments FK_Shipments_Locations_DestinationLocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Locations_DestinationLocationId" FOREIGN KEY ("DestinationLocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: Shipments FK_Shipments_Locations_OriginLocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Locations_OriginLocationId" FOREIGN KEY ("OriginLocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: Shipments FK_Shipments_RouteBlueprints_AssignedRouteId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_RouteBlueprints_AssignedRouteId" FOREIGN KEY ("AssignedRouteId") REFERENCES public."RouteBlueprints"("Id") ON DELETE SET NULL;


--
-- Name: Shipments FK_Shipments_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: Shipments FK_Shipments_Trucks_TruckId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Shipments"
    ADD CONSTRAINT "FK_Shipments_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES public."Trucks"("Id") ON DELETE SET NULL;


--
-- Name: Trucks FK_Trucks_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Trucks"
    ADD CONSTRAINT "FK_Trucks_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: Users FK_Users_Roles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "FK_Users_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE RESTRICT;


--
-- Name: Users FK_Users_Tenants_TenantId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "FK_Users_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES public."Tenants"("Id") ON DELETE RESTRICT;


--
-- Name: WarehouseOperators FK_WarehouseOperators_Employees_EmployeeId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseOperators"
    ADD CONSTRAINT "FK_WarehouseOperators_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES public."Employees"("Id") ON DELETE CASCADE;


--
-- Name: WarehouseOperators FK_WarehouseOperators_Locations_AssignedLocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseOperators"
    ADD CONSTRAINT "FK_WarehouseOperators_Locations_AssignedLocationId" FOREIGN KEY ("AssignedLocationId") REFERENCES public."Locations"("Id") ON DELETE RESTRICT;


--
-- Name: WarehouseOperators FK_WarehouseOperators_WarehouseZones_PrimaryZoneId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseOperators"
    ADD CONSTRAINT "FK_WarehouseOperators_WarehouseZones_PrimaryZoneId" FOREIGN KEY ("PrimaryZoneId") REFERENCES public."WarehouseZones"("Id") ON DELETE SET NULL;


--
-- Name: WarehouseZones FK_WarehouseZones_Locations_LocationId; Type: FK CONSTRAINT; Schema: public; Owner: MetaCodeX
--

ALTER TABLE ONLY public."WarehouseZones"
    ADD CONSTRAINT "FK_WarehouseZones_Locations_LocationId" FOREIGN KEY ("LocationId") REFERENCES public."Locations"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict vPbXrNWUlquOeEJ5QzhfD8JnUNQKiT4WuaC9WX29tPKWN2DF69yiKlPnA6Wcagr

