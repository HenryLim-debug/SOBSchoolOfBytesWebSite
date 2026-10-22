-- 35 fictional, approved demo students. Passwords are BCrypt hashes only.
-- For an existing database this renames the old sobstudentXX accounts in place,
-- preserving IDs, status, and related records. Passwords use FirstName12345.
-- On a fresh database it inserts missing accounts. Safe to run again.
-- The credential CSV is outside this Git repository; do not commit it.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Seed TABLE (
    OldUsername NVARCHAR(50) NOT NULL PRIMARY KEY,
    OldEmail NVARCHAR(254) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(254) NOT NULL,
    OldPasswordHash NVARCHAR(300) NOT NULL,
    NewPasswordHash NVARCHAR(300) NOT NULL
);
INSERT INTO @Seed (OldUsername, OldEmail, FirstName, LastName, Username, Email, OldPasswordHash, NewPasswordHash)
VALUES
    (N'sobstudent06', N'sob.student06@example.test', N'Aisha', N'Rahman', N'Aisha123', N'aisharahman123@gmail.com', N'$2a$12$EhKGCny8rgKfUtj/U2N.J.qv0ifIhydXbMcUU1R0l2Q7QipNzmKpy', N'$2a$12$6.KmNnlPVgpx3O2xnnPeuOxHv0Bigvvi7lXaw3OLduBduTpIpZvdO'),
    (N'sobstudent07', N'sob.student07@example.test', N'Amir', N'Hassan', N'Amir123', N'amirhassan123@gmail.com', N'$2a$12$LWujE3zvPBVGxphh/TQow.v7y.m09reUspJSgj2.5hKCuRbeKw4Ly', N'$2a$12$7TtPqyFDC09RBJhny/d.N.KmDE0JYb8S.T9pLm8g3vv7YONeS.H42'),
    (N'sobstudent08', N'sob.student08@example.test', N'Nur', N'Sofia', N'Nur123', N'nursofia123@gmail.com', N'$2a$12$hK9poKIQNdogsd2J.qxbS.yRXmmsMwMK0lRPiYpMtN4OHEB3xNBTK', N'$2a$12$Av4HyaTS5IEPNK9FdywTVemeT0SVOl6QDmrnX5EC40tE2Vdc1qzK2'),
    (N'sobstudent09', N'sob.student09@example.test', N'Adam', N'Ismail', N'Adam123', N'adamismail123@gmail.com', N'$2a$12$zxGPvJDkR1pXqgDgDVVzNuhcXPO0MBW0t8T1gxVGSeHsLhzV.572K', N'$2a$12$nTJhmqhpghEgKIH2DgpscunNqJsbYtcz2RcY/CrhznjPArEf2w4dO'),
    (N'sobstudent10', N'sob.student10@example.test', N'Mei', N'Ling', N'Mei123', N'meiling123@gmail.com', N'$2a$12$De1VCehGx7O5ilcLA0sZzu8Bg.gGDWN/gj0hCDO1eMYzdxZQiSexe', N'$2a$12$.uNS0o.ZsiU4J/uT1hrJHucK3w8ZptxzZ/aTd4q4QVDtZ90fsPXSm'),
    (N'sobstudent11', N'sob.student11@example.test', N'Arjun', N'Kumar', N'Arjun123', N'arjunkumar123@gmail.com', N'$2a$12$DtyF0oh9nohHpB8cdwR8OOA4GnSJaTtxf4AEgOG.no1WK3kb.Iu5S', N'$2a$12$fQvNdBCiuayWZqYVxMgoKeiKsMhgpcC9ItliTU8hISWVfLo1Q4m2m'),
    (N'sobstudent12', N'sob.student12@example.test', N'Sarah', N'Wong', N'Sarah123', N'sarahwong123@gmail.com', N'$2a$12$BRdrHSi/uyMMW1Crbs0hrudcvGvDFQnI4gBfo21Xvd9lPx8YTk6Ke', N'$2a$12$KaoePi2eGZ4C9SfDxHl0auEOKQEcOpGzS6ywE.6vIqjM1H07LQEna'),
    (N'sobstudent13', N'sob.student13@example.test', N'Daniel', N'Lim', N'Daniel123', N'daniellim123@gmail.com', N'$2a$12$nUNsHMLuEg4.4Mlq.HcdkusSvkZonKMbez.J/opXErct6y2IcTF3C', N'$2a$12$bIqEgKjWt.5TUQeDDDp6teYVe1/EzxMwAGPNYaGNWc7bGs2Ptf2l2'),
    (N'sobstudent14', N'sob.student14@example.test', N'Hana', N'Zainal', N'Hana123', N'hanazainal123@gmail.com', N'$2a$12$BsDAJuTB/ZGO.Oz3pmrpJOVJuQh3OVTCf72/BbC2pADibaKtGugSa', N'$2a$12$3gEhnrxbWofAOyLmEeFNoeWh3MugGFNtVojx.51jRqHcBwEPjQKNW'),
    (N'sobstudent15', N'sob.student15@example.test', N'Ethan', N'Goh', N'Ethan123', N'ethangoh123@gmail.com', N'$2a$12$j0vHUkToBzK1H8OfavcD4uB1TWrIQu/BsrLi2I3cNLly5n86ObXSW', N'$2a$12$VAIAZPJxG6bZ5f.jCxsX0OEs.HyQ8Ozcl8YZMmQFFGxOiSYjX2SJO'),
    (N'sobstudent16', N'sob.student16@example.test', N'Farah', N'Aziz', N'Farah123', N'farahaziz123@gmail.com', N'$2a$12$4orOMsQyU5KNXh/jzToLZ.5//TeRM6y7Pg5rD8Mg9zGxNvyiD64kq', N'$2a$12$PGeniWW9k9oleKJus4VQ1e8oJMyEapAxdgmu6ermKUlmVdVOz6bSW'),
    (N'sobstudent17', N'sob.student17@example.test', N'Ryan', N'Lee', N'Ryan123', N'ryanlee123@gmail.com', N'$2a$12$.MUUzfv6fx7sx3bd73diDOn7gVP7kQnwVl7iRagD4hzXPpOp9s0Na', N'$2a$12$xCkOJGsfpV83nNyUzZPopO4cPupM6tulT2cUr9TFIGO4EpHI/s82i'),
    (N'sobstudent18', N'sob.student18@example.test', N'Priya', N'Nair', N'Priya123', N'priyanair123@gmail.com', N'$2a$12$O/AjiMQgb5vPVFlskIkG4uAXMpzyxRa9P38sBGlwu.dkB2jL9JzHe', N'$2a$12$JeZS5HAzm/ARG3SDlXjeweXMaWiLk3qmUyda.rkuYy.EvDkIRCNT6'),
    (N'sobstudent19', N'sob.student19@example.test', N'Iman', N'Hakim', N'Iman123', N'imanhakim123@gmail.com', N'$2a$12$pN5iQbadB75ViciKhJWhIOZk1mjMyft1XsLU27g4ATjblTh9GepnK', N'$2a$12$/pqC3BHXOOntY2.D0mFzceUbjn8ZTrqhJh5j.yNWFdoGVYMrIY9Qe'),
    (N'sobstudent20', N'sob.student20@example.test', N'Olivia', N'Chan', N'Olivia123', N'oliviachan123@gmail.com', N'$2a$12$0pTlCpHR.iKV/tCUb3hkQegIG7dmvE4JHffu06Fw.1DgemHmIp24y', N'$2a$12$gcLHyKJi1fz4XQ48iMjia.gE3tGbf0jHCw6a6jluTJqL/4P4B/MHS'),
    (N'sobstudent21', N'sob.student21@example.test', N'Yusuf', N'Ibrahim', N'Yusuf123', N'yusufibrahim123@gmail.com', N'$2a$12$k/pb744um9qKyp2VKNVcdutN4jyRddGaIQnCmzBv8hYdlDs2bFtJm', N'$2a$12$U.qQXV7/14rnip3lgJhUNuQe5l22qwqVExP4AgltX.oJC66bdJcaW'),
    (N'sobstudent22', N'sob.student22@example.test', N'Emily', N'Tan', N'Emily123', N'emilytan123@gmail.com', N'$2a$12$dtNY9cKmfHe.MuuJbhfE9e5/pf93IAKzjh5WHh.G9SHPgZc7M1gIy', N'$2a$12$Na7uvyPBzhf1A6Un9FPtGu/x.zRoRQU.QCcG14up6eVg7quPvCF5W'),
    (N'sobstudent23', N'sob.student23@example.test', N'Noah', N'Teo', N'Noah123', N'noahteo123@gmail.com', N'$2a$12$oPI4eOPnRdgWXcbKNuqixeRRqm5vM8OBUQ4HwFKALbHxA49pIvoMK', N'$2a$12$DZEjXRFQd10QE7aN/LFl/uX0DhQHb6Nzl9MAfRBccC.YIophl2NL2'),
    (N'sobstudent24', N'sob.student24@example.test', N'Zara', N'Ahmad', N'Zara123', N'zaraahmad123@gmail.com', N'$2a$12$8RU9alsLf80SIjVXBtAWf.Z351EL6MVm0rNaI8OaCPNmrWBB1ADE2', N'$2a$12$OJyLWZg8RUYgOo0IN6X2MuswyQ1jMqDl38npaIZI6FfO6IPiOdxpm'),
    (N'sobstudent25', N'sob.student25@example.test', N'Isaac', N'Ng', N'Isaac123', N'isaacng123@gmail.com', N'$2a$12$W3I25qZ6Zf.K25gnD.bFxuv92Jb5HVht1sBc51oWSW5D9fMkOR5le', N'$2a$12$SFAI.qcnu47SZWuSNqmfv.WCxu6iex0/92DqAl9/KwVawiIg1bNEi'),
    (N'sobstudent26', N'sob.student26@example.test', N'Alya', N'Hamid', N'Alya123', N'alyahamid123@gmail.com', N'$2a$12$5iIUfe1n7becKxw.zagPMut5Fv5i7zVTuZktWZ42cL0z./SD7.tua', N'$2a$12$QYF3a2f3YGLmX5SSThN7Q.U14ED96D0hDlfVzkHuUI7TD4S95qhXe'),
    (N'sobstudent27', N'sob.student27@example.test', N'Lucas', N'Chua', N'Lucas123', N'lucaschua123@gmail.com', N'$2a$12$BKsbbtMmDMmSBiy/iT/7MO1H1zpOyX23ACXFhzWju3.LHHL2B5ptS', N'$2a$12$WVMEW73C9.ugVo3TJ.jx.uc5bSQdK.JCCkiNc8KUOb7m4Hk1FBGDS'),
    (N'sobstudent28', N'sob.student28@example.test', N'Nisha', N'Devi', N'Nisha123', N'nishadevi123@gmail.com', N'$2a$12$31C6jls2Rq4QKb01JPW/d.CYa9ZiUI.Lh5YlmGMxAOo4jmj0cKBqK', N'$2a$12$3/YotRmouooxCAiqwVuZVORGUjc2.DWjdQY/J3VRaIfgTTPEO9lPi'),
    (N'sobstudent29', N'sob.student29@example.test', N'Benjamin', N'Ho', N'Benjamin123', N'benjaminho123@gmail.com', N'$2a$12$4uV6FxGOrDz3We4/RsIq9uFQvmHug2wb1kdzaxn5krRSiK4RpYGhi', N'$2a$12$DvH7nAK3qr4MFYC.OBAKt.cgqL8mYAs4gH/YcYZl.ZPUSWKMXAfam'),
    (N'sobstudent30', N'sob.student30@example.test', N'Sofia', N'Abdullah', N'Sofia123', N'sofiaabdullah123@gmail.com', N'$2a$12$MpRpnKbtanZK6uZcdvw5w..XA4ufakVmFcAm8rqqUw02SvmPfMPl2', N'$2a$12$Bop/kwN9cSnsDNu1o9paB./cYF1OIMWQu7E6fRUGnauJO0ZtHS9Ie'),
    (N'sobstudent31', N'sob.student31@example.test', N'Aaron', N'Yap', N'Aaron123', N'aaronyap123@gmail.com', N'$2a$12$DM.KbYv7k6aRCDGKX4aEz.ZwWlpZ1ftELVE0lTkAd.nOxYEjAH05W', N'$2a$12$sAFFIHn0WpM9VCouF4fqteBzcriHPO2/hfwoFV214m/C1a4xhCjYK'),
    (N'sobstudent32', N'sob.student32@example.test', N'Nurin', N'Salleh', N'Nurin123', N'nurinsalleh123@gmail.com', N'$2a$12$mkRB3SsgA0p9wlgrRe7MVOCfMN0XkxduRnZ6Z7QtzzK1vEigzl64O', N'$2a$12$FKjP8hSFCMjAH4Q3Wydg6uBZL3bepEtmzq6JevDp0BgoEv5Bopnw.'),
    (N'sobstudent33', N'sob.student33@example.test', N'Dylan', N'Low', N'Dylan123', N'dylanlow123@gmail.com', N'$2a$12$pujQ8GLeJcMm5vcCfK5lGuzooI3QDBCuiCanMlIdmMVmbBwdtlxOu', N'$2a$12$LB.nD45nSrCFG9yHl3bltO2K..8rWzRMD6dVm5mHHVu22yAId3NuG'),
    (N'sobstudent34', N'sob.student34@example.test', N'Kavya', N'Raj', N'Kavya123', N'kavyaraj123@gmail.com', N'$2a$12$lLe/NAdlUSZi8A9MAesYuOh6kbtC0ATyPVMpn8XzV.9Lg7wGE9i1e', N'$2a$12$161RypOPBHHQP4scXmXjy.kiXtsM7YKbzeTlftf66pKUCoOyzLEmS'),
    (N'sobstudent35', N'sob.student35@example.test', N'Grace', N'Ong', N'Grace123', N'graceong123@gmail.com', N'$2a$12$D1QZWNwhD38F2W/OFdHDjuquQ2rMB85JYhWCZH4GDgzCt.1Kij6Ye', N'$2a$12$SqDZaEQXpgU55i0m0gxCF.8uBPcSoUIH/jBoTi6vtu0W60zdInT2O'),
    (N'sobstudent36', N'sob.student36@example.test', N'Harith', N'Malik', N'Harith123', N'harithmalik123@gmail.com', N'$2a$12$9ihEgGdjEAMplGMi2ch6..eHPj.wMU5kisV0/NClzR9DGlxCRgGIm', N'$2a$12$9Y2dpWsAUrHDK0hfQFYr/esHKUSpppsMmvxYU1TtkQTkRSNjBWcF6'),
    (N'sobstudent37', N'sob.student37@example.test', N'Vivian', N'Lau', N'Vivian123', N'vivianlau123@gmail.com', N'$2a$12$ogd1ttFIR2JfjvaDl4Hogu1Iu1OXWMFLgOWLZ9jHQ.ZghlCcqReZq', N'$2a$12$Q3zGl0qMPDsXAss2cdMx2..vo5FVW9FUSIMBkMmrjWf5ESh8SNMyW'),
    (N'sobstudent38', N'sob.student38@example.test', N'Rafael', N'Wong', N'Rafael123', N'rafaelwong123@gmail.com', N'$2a$12$9I8DtnESKpj4XUp1rV/X2OeLC54PgvCii1Au4slFv3mrfiGltFAZq', N'$2a$12$FzWgqqvDR6pyTOu2mQWKVON2YrXLJRwutOfHLCCDZrTndDFxS0B8e'),
    (N'sobstudent39', N'sob.student39@example.test', N'Liyana', N'Musa', N'Liyana123', N'liyanamusa123@gmail.com', N'$2a$12$OsaOUVY0hL8TvIwQRBWv9uQFQgMLK5DobkDgu1fjWGNNxjj2J.od.', N'$2a$12$QgDu8B6xMGge6GdLXI9ItOZLuhTDtyhRYCMYHCR4Q8uEKh8Hcu50.'),
    (N'sobstudent40', N'sob.student40@example.test', N'Jason', N'Chee', N'Jason123', N'jasonchee123@gmail.com', N'$2a$12$bdrx47aFvnXCBXnLDysuqui6ZpkD8t87iAtcFuYXM3Ok7u8FW6ore', N'$2a$12$wmT7332Icg8KB7fKa/DcYO0cIyyRc.sgcBwiOS5WSseubF8GGEQp.');

-- Stop if another account has already claimed one of the requested usernames.
IF EXISTS (
    SELECT 1 FROM @Seed AS s
    JOIN dbo.userTable AS target ON target.username = s.Username
    WHERE target.usertype <> N'Student'
       OR target.fname <> s.FirstName OR target.lname <> s.LastName
       OR target.email <> s.Email
)
    THROW 50001, 'A requested demo username belongs to another account.', 1;
IF EXISTS (
    SELECT 1 FROM @Seed AS s
    JOIN dbo.userTable AS source ON source.username = s.OldUsername
    WHERE source.usertype <> N'Student'
       OR source.fname <> s.FirstName OR source.lname <> s.LastName
)
    THROW 50002, 'An old demo username belongs to another account.', 1;
IF EXISTS (
    SELECT 1 FROM @Seed AS s
    JOIN dbo.userTable AS source ON source.username = s.OldUsername
    JOIN dbo.userTable AS target ON target.username = s.Username AND target.Id <> source.Id
)
    THROW 50003, 'Both old and new demo usernames exist; resolve this before importing.', 1;

-- Preserve each student's existing ID and password. If a recovery email still
-- contains the old demo address, update that address too.
UPDATE u SET
    u.username = s.Username,
    u.email = s.Email,
    u.RecoveryEmail = CASE WHEN u.RecoveryEmail = s.OldEmail THEN s.Email ELSE u.RecoveryEmail END
FROM dbo.userTable AS u
JOIN @Seed AS s ON u.username = s.OldUsername;

-- Change only the original generated demo passwords. Later user resets remain intact.
-- Invalidate old sessions and recovery codes when a password changes.
UPDATE u SET
    u.[password] = s.NewPasswordHash,
    u.SecurityStamp = NEWID(),
    u.RecoveryCodeHash = NULL,
    u.RecoveryCodeCreatedAt = NULL
FROM dbo.userTable AS u
JOIN @Seed AS s ON u.username = s.Username
WHERE u.usertype = N'Student'
  AND u.fname = s.FirstName AND u.lname = s.LastName
  AND u.[password] = s.OldPasswordHash;

INSERT INTO dbo.userTable
    (fname, lname, country, email, username, [password], usertype, staffId, accountStatus)
SELECT s.FirstName, s.LastName, N'Malaysia', s.Email, s.Username, s.NewPasswordHash,
       N'Student', NULL, N'Approved'
FROM @Seed AS s
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.userTable AS u WITH (UPDLOCK, HOLDLOCK)
    WHERE u.username = s.Username
);

IF (SELECT COUNT(*) FROM @Seed AS s JOIN dbo.userTable AS u ON u.username = s.Username) <> 35
    THROW 50004, 'Not all 35 demo student accounts were found after import.', 1;
COMMIT TRANSACTION;

SELECT COUNT(*) AS DemoStudentCount
FROM @Seed AS s JOIN dbo.userTable AS u ON u.username = s.Username;
