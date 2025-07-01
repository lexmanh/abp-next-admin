-- Create database
CREATE DATABASE "Workflow-V70" WITH ENCODING 'UTF8';

-- Connect to the database
\connect "Workflow-V70"

-- Create schema
CREATE SCHEMA IF NOT EXISTS public;

-- Create __EFMigrationsHistory table
CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Begin transaction for main schema
BEGIN;

-- Create Bookmarks table
CREATE TABLE public."Bookmarks" (
    "Id" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "Hash" VARCHAR(255) NOT NULL,
    "Model" TEXT NOT NULL,
    "ModelType" TEXT NOT NULL,
    "ActivityType" VARCHAR(255) NOT NULL,
    "ActivityId" VARCHAR(255) NOT NULL,
    "WorkflowInstanceId" VARCHAR(255) NOT NULL,
    "CorrelationId" VARCHAR(255),
    CONSTRAINT "PK_Bookmarks" PRIMARY KEY ("Id")
);

-- Create WorkflowDefinitions table
CREATE TABLE public."WorkflowDefinitions" (
    "Id" VARCHAR(255) NOT NULL,
    "DefinitionId" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "Name" VARCHAR(255),
    "DisplayName" TEXT,
    "Description" TEXT,
    "Version" INTEGER NOT NULL,
    "IsSingleton" BOOLEAN NOT NULL,
    "PersistenceBehavior" INTEGER NOT NULL,
    "DeleteCompletedInstances" BOOLEAN NOT NULL,
    "IsPublished" BOOLEAN NOT NULL,
    "IsLatest" BOOLEAN NOT NULL,
    "Tag" VARCHAR(255),
    "Data" TEXT,
    CONSTRAINT "PK_WorkflowDefinitions" PRIMARY KEY ("Id")
);

-- Create WorkflowExecutionLogRecords table
CREATE TABLE public."WorkflowExecutionLogRecords" (
    "Id" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "WorkflowInstanceId" VARCHAR(255) NOT NULL,
    "ActivityId" VARCHAR(255) NOT NULL,
    "ActivityType" VARCHAR(255) NOT NULL,
    "Timestamp" TIMESTAMP(6) NOT NULL,
    "EventName" TEXT,
    "Message" TEXT,
    "Source" TEXT,
    "Data" TEXT,
    CONSTRAINT "PK_WorkflowExecutionLogRecords" PRIMARY KEY ("Id")
);

-- Create WorkflowInstances table
CREATE TABLE public."WorkflowInstances" (
    "Id" VARCHAR(255) NOT NULL,
    "DefinitionId" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "Version" INTEGER NOT NULL,
    "WorkflowStatus" INTEGER NOT NULL,
    "CorrelationId" VARCHAR(255),
    "ContextType" VARCHAR(255),
    "ContextId" VARCHAR(255),
    "Name" VARCHAR(255),
    "CreatedAt" TIMESTAMP(6) NOT NULL,
    "LastExecutedAt" TIMESTAMP(6),
    "FinishedAt" TIMESTAMP(6),
    "CancelledAt" TIMESTAMP(6),
    "FaultedAt" TIMESTAMP(6),
    "Data" TEXT,
    CONSTRAINT "PK_WorkflowInstances" PRIMARY KEY ("Id")
);

-- Create indexes for Bookmarks
CREATE INDEX "IX_Bookmark_ActivityId" ON public."Bookmarks" ("ActivityId");
CREATE INDEX "IX_Bookmark_ActivityType" ON public."Bookmarks" ("ActivityType");
CREATE INDEX "IX_Bookmark_ActivityType_TenantId_Hash" ON public."Bookmarks" ("ActivityType", "TenantId", "Hash");
CREATE INDEX "IX_Bookmark_CorrelationId" ON public."Bookmarks" ("CorrelationId");
CREATE INDEX "IX_Bookmark_Hash" ON public."Bookmarks" ("Hash");
CREATE INDEX "IX_Bookmark_Hash_CorrelationId_TenantId" ON public."Bookmarks" ("Hash", "CorrelationId", "TenantId");
CREATE INDEX "IX_Bookmark_TenantId" ON public."Bookmarks" ("TenantId");
CREATE INDEX "IX_Bookmark_WorkflowInstanceId" ON public."Bookmarks" ("WorkflowInstanceId");

-- Create indexes for WorkflowDefinitions
CREATE UNIQUE INDEX "IX_WorkflowDefinition_DefinitionId_VersionId" ON public."WorkflowDefinitions" ("DefinitionId", "Version");
CREATE INDEX "IX_WorkflowDefinition_IsLatest" ON public."WorkflowDefinitions" ("IsLatest");
CREATE INDEX "IX_WorkflowDefinition_IsPublished" ON public."WorkflowDefinitions" ("IsPublished");
CREATE INDEX "IX_WorkflowDefinition_Name" ON public."WorkflowDefinitions" ("Name");
CREATE INDEX "IX_WorkflowDefinition_Tag" ON public."WorkflowDefinitions" ("Tag");
CREATE INDEX "IX_WorkflowDefinition_TenantId" ON public."WorkflowDefinitions" ("TenantId");
CREATE INDEX "IX_WorkflowDefinition_Version" ON public."WorkflowDefinitions" ("Version");

-- Create indexes for WorkflowExecutionLogRecords
CREATE INDEX "IX_WorkflowExecutionLogRecord_ActivityId" ON public."WorkflowExecutionLogRecords" ("ActivityId");
CREATE INDEX "IX_WorkflowExecutionLogRecord_ActivityType" ON public."WorkflowExecutionLogRecords" ("ActivityType");
CREATE INDEX "IX_WorkflowExecutionLogRecord_TenantId" ON public."WorkflowExecutionLogRecords" ("TenantId");
CREATE INDEX "IX_WorkflowExecutionLogRecord_Timestamp" ON public."WorkflowExecutionLogRecords" ("Timestamp");
CREATE INDEX "IX_WorkflowExecutionLogRecord_WorkflowInstanceId" ON public."WorkflowExecutionLogRecords" ("WorkflowInstanceId");

-- Create indexes for WorkflowInstances
CREATE INDEX "IX_WorkflowInstance_ContextId" ON public."WorkflowInstances" ("ContextId");
CREATE INDEX "IX_WorkflowInstance_ContextType" ON public."WorkflowInstances" ("ContextType");
CREATE INDEX "IX_WorkflowInstance_CorrelationId" ON public."WorkflowInstances" ("CorrelationId");
CREATE INDEX "IX_WorkflowInstance_CreatedAt" ON public."WorkflowInstances" ("CreatedAt");
CREATE INDEX "IX_WorkflowInstance_DefinitionId" ON public."WorkflowInstances" ("DefinitionId");
CREATE INDEX "IX_WorkflowInstance_FaultedAt" ON public."WorkflowInstances" ("FaultedAt");
CREATE INDEX "IX_WorkflowInstance_FinishedAt" ON public."WorkflowInstances" ("FinishedAt");
CREATE INDEX "IX_WorkflowInstance_LastExecutedAt" ON public."WorkflowInstances" ("LastExecutedAt");
CREATE INDEX "IX_WorkflowInstance_Name" ON public."WorkflowInstances" ("Name");
CREATE INDEX "IX_WorkflowInstance_TenantId" ON public."WorkflowInstances" ("TenantId");
CREATE INDEX "IX_WorkflowInstance_WorkflowStatus" ON public."WorkflowInstances" ("WorkflowStatus");
CREATE INDEX "IX_WorkflowInstance_WorkflowStatus_DefinitionId" ON public."WorkflowInstances" ("WorkflowStatus", "DefinitionId");
CREATE INDEX "IX_WorkflowInstance_WorkflowStatus_DefinitionId_Version" ON public."WorkflowInstances" ("WorkflowStatus", "DefinitionId", "Version");

-- Insert migration history
INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210523093427_Initial', '5.0.10');

COMMIT;

-- Update 21
BEGIN;

ALTER TABLE public."WorkflowInstances" ALTER COLUMN "CorrelationId" SET NOT NULL;
ALTER TABLE public."WorkflowInstances" ALTER COLUMN "CorrelationId" SET DEFAULT '';
ALTER TABLE public."WorkflowInstances" ADD "LastExecutedActivityId" TEXT;

ALTER TABLE public."WorkflowDefinitions" ADD "OutputStorageProviderName" TEXT;

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210611200027_Update21', '5.0.10');

COMMIT;

-- Update 23
BEGIN;

ALTER TABLE public."WorkflowDefinitions" DROP COLUMN "OutputStorageProviderName";

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210923112211_Update23', '5.0.10');

COMMIT;

-- Update 24
BEGIN;

ALTER TABLE public."WorkflowInstances" ADD "DefinitionVersionId" TEXT NOT NULL DEFAULT '';
ALTER TABLE public."Bookmarks" ALTER COLUMN "CorrelationId" SET NOT NULL;
ALTER TABLE public."Bookmarks" ALTER COLUMN "CorrelationId" SET DEFAULT '';

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20211215100204_Update24', '5.0.10');

COMMIT;

-- Update 241
BEGIN;

ALTER TABLE public."WorkflowInstances" ALTER COLUMN "DefinitionVersionId" TYPE VARCHAR(255);
ALTER TABLE public."WorkflowInstances" ALTER COLUMN "DefinitionVersionId" SET NOT NULL;

CREATE INDEX "IX_WorkflowInstance_DefinitionVersionId" ON public."WorkflowInstances" ("DefinitionVersionId");

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220120170050_Update241', '5.0.10');

COMMIT;

-- Update 25
BEGIN;

CREATE TABLE public."Triggers" (
    "Id" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "Hash" VARCHAR(255) NOT NULL,
    "Model" TEXT NOT NULL,
    "ModelType" TEXT NOT NULL,
    "ActivityType" VARCHAR(255) NOT NULL,
    "ActivityId" VARCHAR(255) NOT NULL,
    "WorkflowDefinitionId" VARCHAR(255) NOT NULL,
    CONSTRAINT "PK_Triggers" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_Trigger_ActivityId" ON public."Triggers" ("ActivityId");
CREATE INDEX "IX_Trigger_ActivityType" ON public."Triggers" ("ActivityType");
CREATE INDEX "IX_Trigger_ActivityType_TenantId_Hash" ON public."Triggers" ("ActivityType", "TenantId", "Hash");
CREATE INDEX "IX_Trigger_Hash" ON public."Triggers" ("Hash");
CREATE INDEX "IX_Trigger_Hash_TenantId" ON public."Triggers" ("Hash", "TenantId");
CREATE INDEX "IX_Trigger_TenantId" ON public."Triggers" ("TenantId");
CREATE INDEX "IX_Trigger_WorkflowDefinitionId" ON public."Triggers" ("WorkflowDefinitionId");

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220120204150_Update25', '5.0.10');

COMMIT;

-- Update 28
BEGIN;

ALTER TABLE public."WorkflowDefinitions" ADD "CreatedAt" TIMESTAMP(6) NOT NULL DEFAULT '0001-01-01 00:00:00';

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20220512203646_Update28', '5.0.10');

COMMIT;

-- Webhooks
BEGIN;

CREATE TABLE public."WebhookDefinitions" (
    "Id" VARCHAR(255) NOT NULL,
    "TenantId" VARCHAR(255),
    "Name" VARCHAR(255) NOT NULL,
    "Path" VARCHAR(255) NOT NULL,
    "Description" VARCHAR(255),
    "PayloadTypeName" VARCHAR(255),
    "IsEnabled" BOOLEAN NOT NULL,
    CONSTRAINT "PK_WebhookDefinitions" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_WebhookDefinition_Description" ON public."WebhookDefinitions" ("Description");
CREATE INDEX "IX_WebhookDefinition_IsEnabled" ON public."WebhookDefinitions" ("IsEnabled");
CREATE INDEX "IX_WebhookDefinition_Name" ON public."WebhookDefinitions" ("Name");
CREATE INDEX "IX_WebhookDefinition_Path" ON public."WebhookDefinitions" ("Path");
CREATE INDEX "IX_WebhookDefinition_PayloadTypeName" ON public."WebhookDefinitions" ("PayloadTypeName");
CREATE INDEX "IX_WebhookDefinition_TenantId" ON public."WebhookDefinitions" ("TenantId");

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210604065041_Initial', '5.0.10');

COMMIT;

-- Workflow Settings
BEGIN;

CREATE TABLE public."WorkflowSettings" (
    "Id" VARCHAR(255) NOT NULL,
    "WorkflowBlueprintId" VARCHAR(255),
    "Key" VARCHAR(255),
    "Value" VARCHAR(255),
    CONSTRAINT "PK_WorkflowSettings" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_WorkflowSetting_Key" ON public."WorkflowSettings" ("Key");
CREATE INDEX "IX_WorkflowSetting_Value" ON public."WorkflowSettings" ("Value");
CREATE INDEX "IX_WorkflowSetting_WorkflowBlueprintId" ON public."WorkflowSettings" ("WorkflowBlueprintId");

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210730112043_Initial', '5.0.10');

COMMIT;