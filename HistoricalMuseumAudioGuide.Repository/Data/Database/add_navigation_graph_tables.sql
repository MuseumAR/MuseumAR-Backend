-- Script bổ sung bảng Waypoints, WaypointEdges và cột DoorWaypointId cho hệ thống Navigation Graph

-- 1. Bổ sung cột DoorWaypointId vào bảng Rooms nếu chưa có
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Rooms') AND name = 'DoorWaypointId'
)
BEGIN
    ALTER TABLE [Rooms] ADD [DoorWaypointId] INT NULL;
END
GO

-- 2. Tạo bảng Waypoints
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Waypoints')
BEGIN
    CREATE TABLE [Waypoints] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MapId] INT NOT NULL,
        [FloorNumber] INT NOT NULL DEFAULT 1,
        [LocationX] FLOAT NOT NULL,
        [LocationY] FLOAT NOT NULL,
        [WaypointType] NVARCHAR(50) NOT NULL DEFAULT 'HALLWAY',
        [RoomId] INT NULL,
        [Code] NVARCHAR(50) NULL,
        [Name] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_Waypoints_MuseumMaps] FOREIGN KEY ([MapId]) REFERENCES [MuseumMaps] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Waypoints_Rooms] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 3. Tạo bảng WaypointEdges
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'WaypointEdges')
BEGIN
    CREATE TABLE [WaypointEdges] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FromWaypointId] INT NOT NULL,
        [ToWaypointId] INT NOT NULL,
        [Distance] FLOAT NOT NULL DEFAULT 1.0,
        [EdgeType] NVARCHAR(50) NOT NULL DEFAULT 'WALK',
        [IsBidirectional] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_WaypointEdges_FromWaypoint] FOREIGN KEY ([FromWaypointId]) REFERENCES [Waypoints] ([Id]),
        CONSTRAINT [FK_WaypointEdges_ToWaypoint] FOREIGN KEY ([ToWaypointId]) REFERENCES [Waypoints] ([Id])
    );
END
GO

-- 4. Thêm khóa ngoại DoorWaypointId vào bảng Rooms
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_Rooms_Waypoints'
)
BEGIN
    ALTER TABLE [Rooms]
    ADD CONSTRAINT [FK_Rooms_Waypoints] FOREIGN KEY ([DoorWaypointId]) REFERENCES [Waypoints] ([Id]) ON DELETE SET NULL;
END
GO
