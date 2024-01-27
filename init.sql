CREATE TABLE Users
(
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) NOT NULL,
    Password NVARCHAR(100) NOT NULL
);

ALTER TABLE Users
ADD CONSTRAINT UC_User_Username UNIQUE (Username);


CREATE TABLE Posts
(
    Id INT PRIMARY KEY IDENTITY,
    UserId INT NOT NULL,
    CreationDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    Text NVARCHAR(MAX) NOT NULL,
    Polarity FLOAT,
    Subjectivity FLOAT,
    CONSTRAINT FK_Posts_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IDX_Posts_CreationDate ON Posts(CreationDate DESC);
