# Test Suite Overview

**Total: 204 tests** across architecture (unit) and integration layers.

Testing framework: **xUnit.net** with **Shouldly** assertions.

---

## Architecture Tests (`Loop.ArchitectureTests`)

### Domain Layer

#### `Domain/Common/EmailTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Create_ShouldFail_WhenEmailIsEmpty` | Passes a whitespace string to `Email.Create`. Asserts that creation fails with error code `"Common.Email.Empty"`, confirming empty emails are rejected at the value-object level. |
| `Create_ShouldFail_WhenEmailIsInvalid` | Passes `"not-an-email"` (missing domain part after `@`) to `Email.Create`. Asserts failure with error code `"Common.Email.Invalid"`, ensuring the regex validation catches malformed addresses. |
| `Create_ShouldNormalizeEmail_WhenEmailIsValid` | Passes `"  test.user@example.com "` (with surrounding whitespace and mixed case) to `Email.Create`. Asserts success and that `Value` is `"TEST.USER@EXAMPLE.COM"`, verifying the email is trimmed and uppercased. |

#### `Domain/Common/MoneyTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `Create_ShouldFail_WhenAmountIsNegative` | Calls `Money.Create(-0.01m)`. Asserts failure with code `"Common.Money.NegativeAmount"`, ensuring negative monetary amounts are rejected. |
| `Create_ShouldRoundAmount_ToTwoDecimalPlaces` | Calls `Money.Create(1.235m)`. Asserts the amount is rounded to `1.24m` (standard rounding), and the currency defaults to `Money.DefaultCurrency`. |
| `Add_ShouldReturnCorrectSum` | Creates two Money values (`10` and `2.5`) and adds them. Asserts the result is `12.5` with the same currency. |
| `Subtract_ShouldFail_WhenResultWouldBeNegative` | Attempts to subtract `5` from `3`. Asserts failure with code `"Common.Money.NegativeResult"`, preventing negative Money results. |
| `Zero_ShouldReturnMoneyWithDefaultCurrency` | Calls `Money.Zero()`. Asserts amount is `0m` and currency is `Money.DefaultCurrency`, providing a canonical zero-value sentinel. |

#### `Domain/Common/PhoneTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Create_ShouldFail_WhenPhoneIsEmpty` | Passes a whitespace string to `Phone.Create`. Asserts failure with code `"Common.Phone.Empty"`, rejecting blank phone numbers. |
| `Create_ShouldFail_WhenPhoneLengthIsOutOfRange` | Passes `"123456"` (only 6 characters, below the minimum of 7). Asserts failure with code `"Common.Phone.InvalidLength"`. |
| `Create_ShouldTrimPhone_WhenPhoneIsValid` | Passes `" 1234567 "` (valid length with surrounding spaces). Asserts success and that `Value` is `"1234567"`, confirming whitespace trimming. |

#### `Domain/Offers/OfferTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `DeactivateAndActivate_ShouldToggleOfferState` | Creates an active offer, calls `Deactivate()` (asserts `IsActive` is false), then `Activate()` (asserts `IsActive` is true). Verifies the offer lifecycle toggle. |
| `Redeem_ShouldCreateRedemption_WhenOfferIsActiveAndInDateRange` | Redeems a valid active offer within its date range. Asserts a `Redemption` is created with all fields matching, and the `Redemptions` collection contains exactly one entry. |
| `Redeem_ShouldThrow_WhenOfferIsInactive` | Deactivates an offer then attempts to redeem. Asserts a `DomainException` with message `"Offer is not active."`. |
| `Redeem_ShouldThrow_WhenOfferIsOutsideActivePeriod` | Creates an offer whose `StartDate` is in the future. Attempts to redeem and asserts `DomainException` with `"Offer is outside its active period."`. |
| `Redeem_ShouldNotAddRedemption_WhenOfferIsInactive` | Deactivates an offer, catches the expected exception during redeem, and asserts that `Redemptions.Count` remains `0` — confirming no side effects on failure. |

#### `Domain/Receipts/ReceiptTests.cs` (6 tests)
| Test | Description |
|------|-------------|
| `Upload_ShouldCreatePendingReceipt_WithGeneratedIdAndDefaultImageHash` | Calls `Receipt.Upload` with default parameters. Asserts `ReceiptId` is a non-empty GUID, all fields match the inputs, status is `Pending`, and `ImageHash` defaults to `string.Empty`. |
| `Upload_ShouldUseProvidedReceiptIdAndImageHash_WhenProvided` | Calls `Receipt.Upload` with explicit `receiptId` and `imageHash` values. Asserts those exact values are stored, confirming custom identifiers are respected. |
| `Approve_ShouldSetStatusToApproved_WhenReceiptIsPending` | Approves a pending receipt. Asserts the status transitions from `Pending` to `Approved`. |
| `Approve_ShouldThrowDomainException_WhenReceiptIsNotPending` | Rejects a receipt first, then attempts to approve it. Asserts a `DomainException` with message `"Only pending receipts can be approved."`. |
| `Reject_ShouldSetStatusToRejected_WhenReceiptIsPending` | Rejects a pending receipt. Asserts the status becomes `Rejected`. |
| `Reject_ShouldThrowDomainException_WhenReceiptIsNotPending` | Approves a receipt first, then attempts to reject it. Asserts a `DomainException` with message `"Only pending receipts can be rejected."`. |

#### `Domain/Stamps/StampTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Create_ShouldThrow_WhenStampsRequiredIsNotPositive` | Attempts to create a `Stamp` with `stampsRequired = 0`. Asserts a `DomainException` with message `"Stamps required must be greater than zero."`. |
| `Create_ShouldInitializeActiveStamp_WhenInputsAreValid` | Creates a valid `Stamp` and asserts `StampId` is non-empty, `IsActive` is true, and all scalar properties match the constructor arguments. |
| `DeactivateAndActivate_ShouldToggleStampState` | Calls `Deactivate()` then `Activate()` on a stamp, asserting `IsActive` flips to false and back to true. |

#### `Domain/Stamps/StampRedemptionTests.cs` (1 test)
| Test | Description |
|------|-------------|
| `Create_ShouldSetAllFields` | Calls `StampRedemption.Create` with explicit IDs. Asserts `RedemptionId` is generated, all foreign-key IDs match, and `CreatedAt` is not in the future. |

#### `Domain/Stamps/StampTransactionTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `RecordCollect_ShouldSetCollectTransactionFields` | Records a collect transaction with `StampType.Collect` and `stampsCount = 3`. Asserts the type, count, and a non-null `RedemptionRef` (QR ID). |
| `RecordReward_ShouldSetRewardTransactionFields` | Records a reward transaction with `StampType.Reward`. Asserts the type, `stampsCount` is 0, and `RedemptionRef` is null. |

#### `Domain/Stamps/UserStampCardTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Open_ShouldInitializeAnUncompletedCard` | Opens a new `UserStampCard`. Asserts `StampsCounter` starts at 0, `IsCompleted` is false, and timestamps are set. |
| `CollectStamp_ShouldIncrementCounterAndMarkCompleted_WhenThresholdIsReached` | Collects 2 stamps on a card requiring 3 (not yet completed), then collects 1 more. Asserts the counter reaches 3 and `IsCompleted` becomes true. |
| `CollectStamp_ShouldThrow_WhenCountIsNotPositive` [Theory] | Attempts to collect with `count = 0` and `count = -1`. Both throw `DomainException` with message `"Stamp count must be positive."`, and the card state remains unchanged. |
| `CollectStamp_ShouldThrow_WhenCardIsAlreadyCompleted` | Completes a card by collecting the required stamps, then attempts another collect. Asserts `DomainException` with `"Stamp card is already completed."`. |

#### `Domain/Transactions/EarnTransactionTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `Record_ShouldSetAllFields_AndTimestamp_WhenTransactionRefIsProvided` | Records an earn transaction with a non-null `TransactionRef`. Asserts all fields (earn ID, user, shop, amount, points, ref) are correctly set and `CreatedAt` is not in the future. |
| `Record_ShouldAllowNullTransactionRef` | Records an earn transaction with a null ref. Asserts `TransactionRef` is null and `CreatedAt` is populated, confirming the ref is optional. |

#### `Domain/Transactions/RedeemTransactionTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `Initiate_ShouldCreatePendingTransaction_WithVerificationCodeAndTimestamp` | Initiates a redeem transaction. Asserts status is `Pending`, `CompletedAt` is null, a 6-digit numeric verification code is generated, and all value fields match. |
| `Verify_ShouldSucceed_WhenStatusIsPending` | Verifies a pending transaction. Asserts `IsSuccess`, status becomes `Verified`, and `CompletedAt` is populated. |
| `Verify_ShouldFail_WhenTransactionAlreadyProcessed` | Verifies a transaction twice. The second call fails with code `"Transactions.RedemptionAlreadyProcessed"` and the `CompletedAt` timestamp is preserved. |
| `Cancel_ShouldSucceed_WhenStatusIsPending` | Cancels a pending transaction. Asserts `IsSuccess`, status becomes `Cancelled`, and `CompletedAt` is set. |
| `Cancel_ShouldFail_WhenTransactionAlreadyProcessed` | Cancels a transaction twice. The second call fails with `"Transactions.RedemptionAlreadyProcessed"` and the original `CompletedAt` is preserved. |

#### `Domain/Users/UserTests.cs` (11 tests)
| Test | Description |
|------|-------------|
| `Create_ShouldInitializeUserAndPointsBalance` | Creates a `User` with valid inputs. Asserts all identity fields match, `Gender` is set, `TierId` matches the default, `CreatedAt` is recent, and `PointsBalance` is initialized with zero points linked to the user. |
| `CreditPoints_ShouldFail_WhenAmountIsNotPositive` [Theory] | Attempts to credit `0` and `-1` points. Both fail with code `"Users.InvalidCreditAmount"` and the balance remains unchanged. |
| `CreditPoints_ShouldIncreaseTotalAndLifetimePoints_WhenAmountIsPositive` | Credits 25 points. Asserts both `TotalPoints` and `LifetimePoints` become 25, and `LastUpdated` advances. |
| `DebitPoints_ShouldFail_WhenAmountIsNotPositive` [Theory] | Attempts to debit `0` and `-1` points. Both fail with code `"Users.InvalidDebitAmount"`. |
| `DebitPoints_ShouldThrowDomainException_WhenBalanceIsInsufficient` | Credits 10 points then attempts to debit 11. Asserts a `DomainException` is thrown, preventing overdraft. |
| `DebitPoints_ShouldDecreaseTotalPoints_WhenBalanceIsSufficient` | Credits 40 points then debits 15. Asserts `TotalPoints` is 25, `LifetimePoints` stays at 40, and `LastUpdated` advances. |
| `UpdateProfile_ShouldUpdateProfileWithoutChangingPhone_WhenPhoneIsNotProvided` | Calls `UpdateProfile` with new name and image but no phone. Asserts the name and image are updated while `Phone` remains the original. |
| `UpdateProfile_ShouldUpdatePhone_WhenPhoneIsProvided` | Calls `UpdateProfile` with a new `Phone` value object. Asserts the phone is updated along with the other profile fields. |
| `ChangePasswordHash_ShouldSetNewHash` | Calls `ChangePasswordHash("new-hash")`. Asserts `PasswordHash` is replaced. |
| `UpgradeTier_ShouldSetNewTierId` | Calls `UpgradeTier(newTierId)`. Asserts `TierId` is updated. |
| `HasEnoughPoints_ShouldReflectCurrentBalance` | Credits 30 points. Asserts `HasEnoughPoints(30)` returns true and `HasEnoughPoints(31)` returns false. |

---

### Application Layer

#### `Application/Categories/GetCategoriesQueryTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnCategoriesForMall` | Seeds two categories (Coffee, Bakery) for a mall. Calls the handler and asserts both are returned with their IDs and names correctly mapped. |
| `Handle_ShouldReturnEmptyList_WhenMallHasNoCategories` | Seeds an empty category repository. Calls the handler and asserts the result is an empty list. |

#### `Application/Offers/GetOfferQueriesTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `GetOfferById_ShouldReturnMappedResponse_WhenOfferExists` | Creates an offer with a shop attached via reflection. Calls `GetOfferById` and asserts all mapped fields (offer ID, name, description, image, reward type/value, shop ID, shop name, cover image) are correct. |
| `GetOfferById_ShouldReturnNotFound_WhenOfferDoesNotExist` | Calls `GetOfferById` with a random GUID on an empty repository. Asserts failure with code `"Offers.NotFound"`. |
| `GetOffersByShop_ShouldExcludeConfirmedRedemptions` | Creates two offers for the same shop: one unredeemed and one redeemed+confirmed by the current user. Calls `GetOffersByShop` and asserts only the unredeemed offer is returned. |

#### `Application/Queries/GetUserByIdQueryTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnUnauthorized_WhenRequestedUserIsNotCurrentUser` | Creates a handler with `IUserContext` returning userId `111...` and queries for userId `222...`. Asserts failure with code `"Users.Unauthorized"`, preventing users from viewing other profiles. |
| `Handle_ShouldReturnNotFound_WhenUserDoesNotExist` | Sets `IUserContext.UserId` matching the query but seeds an empty repository. Asserts failure with `"Users.NotFound"`. |
| `Handle_ShouldReturnUserResponse_WhenUserExists` | Seeds a user whose ID matches `IUserContext.UserId`. Asserts success with a mapped `UserResponse` containing ID, first name, last name, and email. |

#### `Application/Receipts/LocalMerchantMatcherTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `MatchAsync_ShouldMarkReceiptAsPending_WhenMatchScoreIsBelowMinimumThreshold` | Seeds a shop named "Star Market" and passes an OCR result with unrelated merchant names. Asserts `IsPendingReview` is true, a shop is matched, and the match score is below 0.35 (requiring manual review). |
| `MatchAsync_ShouldNotMarkReceiptAsPending_WhenMatchScoreIsAboveMinimumThreshold` | Seeds a shop named "Carrefour" and passes an exact-match merchant name. Asserts `IsPendingReview` is false and the match score is 1.0 (perfect match). |
| `MatchAsync_ShouldReturnNormalizedReceipt_WhenMerchantIdentityIsMissing` | Seeds no shops, passes an OCR result with blank/null names. Asserts the result normalizes store/merchant names to empty strings, trims the currency, and returns no match. |
| `MatchAsync_ShouldReturnReceiptWithoutMatch_WhenNoShopsExist` | Seeds no shops, passes a valid merchant name. Asserts no match is found, no score is assigned, and the receipt is not marked pending. |
| `MatchAsync_ShouldMatchNamesWithDiacriticsNormalization` | Seeds a shop named "CAFÉ MALL" and passes "Cafe Mall". Asserts a match is found with score 1.0, confirming diacritics are normalized during comparison. |

#### `Application/Shops/ConfirmPointsRedemptionQrCommandTests.cs` (11 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnQrCodeNotFound_WhenQrCodeDoesNotExist` | Creates an empty QR code repository. Calls the handler with a random GUID. Asserts failure with code `"QrCodes.NotFound"`. |
| `Handle_ShouldReturnInvalidQrPayload_WhenPayloadIsNull` | Creates a QR code but configures the token provider to return null. Asserts failure with `"Transactions.InvalidQrPayload"`. |
| `Handle_ShouldReturnInvalidQrPayload_WhenUserIdMismatch` | Creates a QR code and payload where the payload's `UserId` differs from the QR code's `UserId`. Asserts failure with `"Transactions.InvalidQrPayload"`. |
| `Handle_ShouldReturnInvalidQrPayload_WhenShopIdMismatch` | Creates a QR code for Shop A but sets the authenticated admin's context to a different shop (Shop B). The handler detects that `qrCode.ShopId != shopAdminContext.ShopId` and rejects the redemption. Asserts failure with `"Transactions.InvalidQrPayload"`, preventing QR codes generated for one shop from being redeemed at another. |
| `Handle_ShouldReturnQrCodeExpired_WhenPayloadIsExpired` | Creates an expired QR code/payload. Asserts failure with `"Transactions.QrCodeExpired"`. |
| `Handle_ShouldReturnShopNotFound_WhenShopDoesNotExist` | Seeds no shop but uses a valid QR/payload. The handler looks up the shop by `shopAdminContext.ShopId`. Asserts failure with `"Shops.NotFound"`. |
| `Handle_ShouldReturnConfigNotFound_WhenSystemConfigDoesNotExist` | Seeded shop exists but no `SystemConfig` for the mall. Asserts failure with `"Configuration.NotFound"`. |
| `Handle_ShouldReturnUserNotFound_WhenUserDoesNotExist` | Seeds shop + config but no user. Asserts failure with `"Users.NotFound"`. |
| `Handle_ShouldReturnBelowMinThreshold_WhenUserPointsAreBelowMinRedemptionThreshold` | Seeds user with 0 points but the config's minimum threshold is higher. Asserts failure with `"Transactions.BelowMinRedemptionThreshold"`. |
| `Handle_ShouldReturnInsufficientPoints_WhenUserDoesNotHaveEnoughPoints` | Seeds user with 50 points, requests 500. Asserts failure with `"Transactions.InsufficientPoints"`. |
| `Handle_ShouldSucceed_WhenAllConditionsAreMet` | Seeds all dependencies with valid data: QR code, shop, config, user with 200 points requesting 50. Asserts success (true) and that the points are correctly debited. |

#### `Application/Shops/GetShopByIdQueryTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnNotFound_WhenShopDoesNotExist` | Seeds an empty repository. Asserts failure with `"Shops.NotFound"`. |
| `Handle_ShouldReturnNotFound_WhenShopIsNotActive` | Seeds an inactive shop with a category attached via reflection. Asserts failure with `"Shops.NotFound"`, confirming inactive shops are hidden. |
| `Handle_ShouldReturnShopResponse_WhenShopExistsAndIsActive` | Seeds an active shop with a category, social links JSON, and website. Asserts all mapped fields (name, category name, cover, logo, website, social links) are correct. |
| `Handle_ShouldReturnEmptySocialLinks_WhenSocialLinksJsonIsNull` | Seeds an active shop with null social links and website. Asserts `SocialLinks` is empty and `WebsiteLink` is null. |

#### `Application/Shops/GetShopsQueryTests.cs` (7 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnValidationError_WhenPageNumberIsLessThanOne` | Passes `pageNumber: 0`. Asserts failure before any repository access with `"Pagination.InvalidPageNumber"`. |
| `Handle_ShouldReturnValidationError_WhenPageSizeIsLessThanOne` | Passes `pageSize: 0`. Asserts failure with `"Pagination.InvalidPageSize"`. |
| `Handle_ShouldReturnValidationError_WhenPageSizeIsGreaterThanOneHundred` | Passes `pageSize: 101`. Asserts failure with `"Pagination.InvalidPageSize"`. |
| `Handle_ShouldReturnPaginatedShops_WhenShopsExist` | Seeds 5 shops, queries page 1 with page size 3. Asserts `TotalCount` is 5, `TotalPages` is 2, and `Data` has 3 items. |
| `Handle_ShouldReturnEmptyData_WhenNoShopsExist` | Seeds no shops. Asserts success with empty data, `TotalCount` 0, and `TotalPages` 0. |
| `Handle_ShouldFilterByCategoryId` | Seeds two shops with different categories. Filters by coffee category. Asserts only the coffee shop is returned. |
| `Handle_ShouldExcludeInactiveShops` | Seeds one active and one inactive shop. Asserts only the active shop appears in results. |

#### `Application/Shops/LoginShopAdminCommandTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnValidationError_WhenEmailIsInvalid` | Passes `"not-an-email"`. The handler validates email format before querying the repository. Asserts failure with `"Common.Email.Invalid"`. |
| `Handle_ShouldReturnNotFound_WhenAdminDoesNotExist` | Seeds an empty admin repository. Asserts failure with `"Shops.AdminNotFoundByEmail"`. |
| `Handle_ShouldReturnNotFound_WhenPasswordIsWrong` | Seeds an admin but configures the password hasher stub to return `false` (wrong password). Asserts failure with `"Shops.AdminNotFoundByEmail"` (same generic error for security). |
| `Handle_ShouldReturnTokens_WhenLoginSucceeds` | Seeds an admin with matching password. Asserts success with `AuthTokensResponse` containing the expected access and refresh tokens. |

#### `Application/Shops/RefreshShopAdminTokenCommandTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnInvalidToken_WhenSessionNotFound` | Seeds an empty session repository. Asserts failure with `"Shops.InvalidRefreshToken"`. |
| `Handle_ShouldReturnTokenExpired_WhenSessionIsExpired` | Seeds a session whose `ExpiresAtUtc` is set to yesterday via reflection. Asserts failure with `"Shops.RefreshTokenExpired"`. |
| `Handle_ShouldReturnInvalidToken_WhenShopAdminNotFound` | Seeds a valid session but an empty admin repository (the admin was deleted). Asserts failure with `"Shops.InvalidRefreshToken"`. |
| `Handle_ShouldReturnNewTokens_WhenRefreshSucceeds` | Seeds a valid session and matching admin. Asserts success with `AuthTokensResponse` containing new access and refresh tokens. |

#### `Application/Stamps/GetActiveShopStampsQueryTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnActiveStamps_WhenStampsExist` | Seeds a shop with an active, in-date-range stamp. Calls `GetActiveShopStamps` and asserts the stamp is returned with correct ID, name, description, icon URL, stamps required, and reward type. |
| `Handle_ShouldReturnEmptyList_WhenNoActiveStamps` | Seeds an empty stamp repository. Asserts the result is an empty list. |

#### `Application/Stamps/GetUserStampCardsQueryTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldReturnUserStampCards_WhenCardsExist` | Seeds one incomplete and one completed `UserStampCard` for the current user, with a `Stamp` navigation property set via reflection. Asserts both cards are returned with mapped properties including stamp details and shop info. |
| `Handle_ShouldReturnEmptyList_WhenNoCards` | Seeds no cards. Asserts the result is an empty list. |

#### `Application/Stamps/GenerateStampCollectQrCommandTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenStampsCountIsZeroOrNegative` | Passes `stampsCount: 0`. The handler validates the count before any repository access. Asserts failure with `"Stamps.InvalidQrPayload"`. |
| `Handle_ShouldFail_WhenStampNotFound` | Passes a valid count but seeds an empty stamp repository. Asserts failure with `StampErrors.NotFound` code. |
| `Handle_ShouldSucceed_WhenValidRequest` | Seeds an active stamp and a valid count. Asserts success with a `GenerateStampCollectQrResponse` containing a non-empty `QrId` and `ExpiresAtUtc` set to 2 minutes from now. |

#### `Application/Stamps/ScanStampCollectQrCommandTests.cs` (9 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenQrCodeNotFound` | Seeds an empty QR repository. Asserts failure with `StampErrors.QrCodeNotFound`. |
| `Handle_ShouldFail_WhenQrPayloadIsInvalid` | Seeds a QR code but configures the token provider to return null. Asserts failure with `StampErrors.InvalidQrPayload`. |
| `Handle_ShouldFail_WhenQrCodeExpired` | Seeds an expired QR code and an expired token payload. Asserts failure with `StampErrors.QrCodeExpired`. |
| `Handle_ShouldFail_WhenQrCodeAlreadyUsed` | Seeds a QR code and a `StampTransaction` that already references the same `QrId` (via `RedemptionRef`). Asserts failure with `StampErrors.QrCodeAlreadyUsed`. |
| `Handle_ShouldFail_WhenStampNotFound` | Seeds a valid QR but an empty stamp repository. The token's `StampId` cannot be resolved. Asserts failure with `StampErrors.NotFound` code. |
| `Handle_ShouldFail_WhenStampIsInactive` | Seeds a valid QR and a deactivated stamp whose `IsActive` is `false`. The `ActiveStampsByShopSpecification` filters it out, so the handler cannot find it. Asserts failure with `StampErrors.NotFound` code, confirming inactive stamps are treated the same as non-existent ones. |
| `Handle_ShouldFail_WhenCardAlreadyCompleted` | Seeds a valid QR, stamp, and a `UserStampCard` that is already completed. Asserts failure with `StampErrors.CardAlreadyCompleted`. |
| `Handle_ShouldSucceed_CreatingNewCard` | Seeds a valid QR and stamp but no existing `UserStampCard`. The handler creates a new card. Asserts success with `stampsCounter = 1` and `isCompleted = false`. |
| `Handle_ShouldSucceed_WithExistingCard` | Seeds a valid QR, stamp, and an existing incomplete `UserStampCard` with 2 stamps. The handler increments to 3. Asserts success with `stampsCounter = 3`. |

#### `Application/Stamps/GenerateStampRedemptionQrCommandTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenCardNotFound` | Seeds no `UserStampCard` for the requesting user/stamp. The handler finds no matching card. Asserts failure with `StampErrors.CardNotFound` code. |
| `Handle_ShouldFail_WhenCardNotCompleted` | Seeds an incomplete `UserStampCard` (counter < required). Asserts failure with `StampErrors.CardNotCompleted`. |
| `Handle_ShouldSucceed_WhenCardIsCompleted` | Seeds a completed `UserStampCard` (all stamps collected). The handler generates a QR code via the token provider and persists it. Asserts success with a non-empty `QrId` and `ExpiresAtUtc` set to 2 minutes from now. |

#### `Application/Stamps/ConfirmStampRedemptionQrCommandTests.cs` (8 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenQrCodeNotFound` | Seeds an empty QR repository. Asserts failure with `StampErrors.QrCodeNotFound`. |
| `Handle_ShouldFail_WhenQrPayloadIsInvalid` | Seeds a QR code but configures the token provider to return null. Asserts failure with `StampErrors.InvalidQrPayload`. |
| `Handle_ShouldFail_WhenShopIdMismatch` | Seeds a QR code with a valid payload where `payload.ShopId` differs from the admin context's `ShopId`. Asserts failure with `StampErrors.InvalidQrPayload`, preventing redemption at the wrong shop. |
| `Handle_ShouldFail_WhenQrCodeExpired` | Seeds an expired QR code (past `ExpiresAt`) and an expired payload. Asserts failure with `StampErrors.QrCodeExpired`. |
| `Handle_ShouldFail_WhenQrCodeAlreadyUsed` | Seeds a QR code and an existing `StampRedemption` referencing the same `QrId`. Asserts failure with `StampErrors.QrCodeAlreadyUsed`. |
| `Handle_ShouldFail_WhenCardNotFound` | Seeds a valid QR and payload but no `UserStampCard` matching the payload's user and stamp. Asserts failure with `StampErrors.CardNotFound` code. |
| `Handle_ShouldFail_WhenCardNotCompleted` | Seeds a valid QR, payload, and incomplete `UserStampCard`. Asserts failure with `StampErrors.CardNotCompleted`. |
| `Handle_ShouldSucceed_WhenValid` | Seeds all valid dependencies: a non-expired QR, matching payload, no prior redemption, and a completed `UserStampCard`. The handler invalidates the QR and creates a `StampRedemption`. Asserts success and returns `true`. |

#### `Application/Users/UserQueryTests.cs` (6 tests)
| Test | Description |
|------|-------------|
| `GetUserByEmail_ShouldReturnValidationError_WhenEmailIsInvalid` | Passes `"not-an-email"`. The handler validates the email format before querying. Asserts failure with `"Common.Email.Invalid"`. |
| `GetUserByEmail_ShouldReturnUnauthorized_WhenQueryUserIsNotCurrentUser` | Seeds a user but sets `IUserContext.UserId` to a different GUID. Asserts failure with `"Users.Unauthorized"`, preventing users from querying other users' profiles by email. |
| `GetUserByEmail_ShouldReturnUserResponse_WhenUserExistsAndMatchesCurrentUser` | Seeds a user whose ID matches `IUserContext.UserId`. Asserts success and returns a mapped `UserResponse`. |
| `GetUserPointsBalance_ShouldReturnNotFound_WhenUserDoesNotExist` | Seeds no user. Asserts failure with `"Users.NotFound"`. |
| `GetUserPointsBalance_ShouldReturnNotFound_WhenSystemConfigDoesNotExist` | Seeds a user but no `SystemConfig` for the mall. Asserts failure with `"SystemConfig.NotFound"`. |
| `GetUserPointsBalance_ShouldReturnCalculatedBalance_WhenDataExists` | Seeds a user, shop, config, and an `EarnTransaction`. Asserts the balance is correctly calculated from the transaction data and the response includes the user ID, mall ID, max redeemable points, and points-to-currency ratio. |

#### `Application/Users/AddPointsToUserCommandTests.cs` (3 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenAmountIsZeroOrNegative` | Passes `amount = 0`. The handler validates the amount before any repository access. Asserts failure with `"Transactions.InvalidAmount"`. |
| `Handle_ShouldFail_WhenUserNotFound` | Seeds an empty user repository. Asserts failure with `"Users.NotFound"`. |
| `Handle_ShouldSucceed_WhenValid` | Seeds a user and passes a positive amount. Asserts the user's `PointsBalance.TotalPoints` is credited by the expected amount. |

#### `Application/Users/LoginUserCommandTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenEmailIsInvalid` | Passes `"not-an-email"`. The handler validates email format before querying. Asserts failure with `"Common.Email.Invalid"`. |
| `Handle_ShouldFail_WhenUserNotFound` | Seeds an empty user repository. Asserts failure with `"Users.NotFound"` (generic error, same as wrong password — prevents user enumeration). |
| `Handle_ShouldFail_WhenPasswordIsWrong` | Seeds a user but configures the password hasher stub to return false. Asserts failure with `"Users.NotFound"` (same generic error). |
| `Handle_ShouldSucceed_WhenCredentialsAreCorrect` | Seeds a user with matching password hasher. Asserts success with `AuthTokensResponse` containing access and refresh tokens, and a session is persisted. |

#### `Application/Users/LogoutUserCommandTests.cs` (2 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenUserNotFound` | Seeds an empty user repository. Asserts failure with `"Users.NotFound"`. |
| `Handle_ShouldSucceed_WhenUserExists` | Seeds a user with multiple active sessions. Asserts success and that all sessions for the user are deleted from the repository. |

#### `Application/Users/RegisterUserCommandTests.cs` (6 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenEmailIsInvalid` | Passes `"not-an-email"`. The handler validates email format before any repository access. Asserts failure with `"Common.Email.Invalid"`. |
| `Handle_ShouldFail_WhenEmailNotUnique` | Seeds an existing user with the same email. The handler detects the conflict via `Find + AnyAsync`. Asserts failure with `UserErrors.EmailNotUnique`. |
| `Handle_ShouldFail_WhenPhoneIsInvalid` | Passes `"ab"` (too short). The handler validates phone format after email uniqueness. Asserts failure with `"Common.Phone.InvalidLength"`. |
| `Handle_ShouldFail_WhenGenderIsInvalid` | Passes `"InvalidGender"`. After email and phone pass, the handler tries to parse the gender enum and fails. Asserts failure with `UserErrors.InvalidGender`. |
| `Handle_ShouldFail_WhenDefaultTierNotFound` | Seeds no default tier (Order=1). The handler uses `TierByOrderSpecification(1)` to find the default tier. Asserts failure with `TierErrors.NotFound(1)` code. |
| `Handle_ShouldSucceed_WhenValidInput` | Seeds no conflicting users and a default tier. Asserts success and returns a non-empty `Guid` representing the new user's ID. |

#### `Application/Users/GeneratePointsRedemptionQrCommandTests.cs` (4 tests)
| Test | Description |
|------|-------------|
| `Handle_ShouldFail_WhenPointsToRedeemIsZeroOrNegative` | Passes `pointsToRedeem = 0`. The handler validates the amount before querying. Asserts failure with `"Users.InvalidRedemptionPoints"`. |
| `Handle_ShouldFail_WhenUserNotFound` | Seeds no user. Asserts failure with `"Users.NotFound"`. |
| `Handle_ShouldFail_WhenInsufficientPoints` | Seeds a user with 0 points who requests 100. Asserts failure with `UserErrors.InsufficientPoints`. |
| `Handle_ShouldSucceed_WhenSufficientPoints` | Seeds a user with 200 points who requests 100. Asserts success with a non-empty `QrId` and `ExpiresAtUtc` set to 2 minutes from now, confirming the QR code is persisted and points are debited. |

---

### Infrastructure Layer

#### `Infrastructure/Ocr/MestalOcrProviderTests.cs` (13 tests)
| Test | Description |
|------|-------------|
| `ProcessAsync_ShouldReturnFailure_WhenHttpRequestFails` | Configures the `HttpClient` mock to return a non-success status code. Asserts the OCR result is a failure. |
| `ProcessAsync_ShouldParseStructuredAnnotation_WhenAnnotationContainsValidJson` | Returns a response with a valid JSON annotation block matching the expected schema. Asserts the structured fields (store name, merchant name, subtotal, currency) are correctly parsed. |
| `ProcessAsync_ShouldParseStructuredAnnotation_WhenFallbackAnnotationPropertyExists` | Returns a response where the primary annotation property is missing but a fallback annotation property exists with valid JSON. Asserts parsing succeeds from the fallback. |
| `ProcessAsync_ShouldParseStructuredAnnotation_WhenAnnotationIsWrappedInCodeFences` | Returns a response where the annotation JSON is wrapped inside markdown code fences (````json ... ````). Asserts the fences are stripped and parsing succeeds. |
| `ProcessAsync_ShouldFallbackToMarkdownFirstLine_WhenStructuredAnnotationIsMissing` | Returns a response with no annotation JSON. Asserts the OCR result falls back to extracting the first line of the markdown body as `storeName`. |
| `ProcessAsync_ShouldFallbackToTextProperty_WhenPagesAreMissing` | Returns a response with no pages array. Asserts the result falls back to the top-level `text` property for content extraction. |
| `ProcessAsync_ShouldReturnInvalidResponse_WhenAnnotationCannotBeDeserialized` | Returns an annotation with malformed JSON. Asserts the result is marked invalid with the raw body preserved. |
| `ProcessAsync_ShouldReturnInvalidResponse_WhenResponseBodyIsNotJson` | Returns a plain-text response body (not JSON). Asserts the deserialization fails and the result is invalid. |
| `ProcessAsync_ShouldReturnFailure_WhenHttpClientThrowsHttpRequestException` | Configures the `HttpClient` mock to throw `HttpRequestException`. Asserts the result is a failure. |
| `ProcessAsync_ShouldReturnFailure_WhenRequestIsCanceled` | Passes a cancelled `CancellationToken`. Asserts the result is a failure and a `TaskCanceledException` is observed. |
| `ProcessAsync_ShouldRethrowTaskCanceledException_WhenTokenWasNotCanceled` | Configures the `HttpClient` to throw `TaskCanceledException` without cancelling the token (unexpected cancellation). Asserts the exception is rethrown rather than swallowed. |
| `ProcessAsync_ShouldNormalizeMimeType_WhenContentTypeContainsParameters` | Passes a base64 image with `ContentType` containing parameters (e.g., `image/jpeg; name=file.jpg`). Asserts the MIME type is normalized to the base type (`image/jpeg`). |
| `ProcessAsync_ShouldDefaultMimeTypeToJpeg_WhenContentTypeIsBlank` | Passes a base64 image with a blank `ContentType`. Asserts the MIME type defaults to `image/jpeg`. |

---

### Architecture Layer Constraints

#### `Layers/LayerTests.cs` (6 tests)
| Test | Description |
|------|-------------|
| `DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer` | Uses NetArchTest to assert no types in the Domain layer reference any type from the Application layer. Preserves domain purity. |
| `DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer` | Asserts the Domain layer has no references to the Infrastructure layer. |
| `DomainLayer_ShouldNotHaveDependencyOn_PresentationLayer` | Asserts the Domain layer has no references to the Presentation layer. |
| `ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer` | Asserts the Application layer depends only on abstractions, not on concrete infrastructure implementations. |
| `ApplicationLayer_ShouldNotHaveDependencyOn_PresentationLayer` | Asserts the Application layer has no references to the Presentation layer. |
| `InfrastructureLayer_ShouldNotHaveDependencyOn_PresentationLayer` | Asserts the Infrastructure layer has no references to the Presentation layer. |

#### `Layers/ApplicationLayerTests.cs` (11 tests)
| Test | Description |
|------|-------------|
| `Commands_ShouldBe_Sealed` | Asserts every class whose name ends with `Command` (and is not a handler) is declared `sealed`. |
| `Commands_ShouldHave_CommandSuffix` | Asserts all command records follow the naming convention, ending with `Command`. |
| `Queries_ShouldBe_Sealed` | Asserts all query classes are sealed. |
| `Queries_ShouldHave_QuerySuffix` | Asserts all query classes end with `Query`. |
| `CommandHandlers_ShouldBe_Sealed` | Asserts all command handler implementations are sealed. |
| `CommandHandlers_ShouldResideIn_CommandNamespace` | Asserts all command handlers are located under a `*.Command` namespace. |
| `QueryHandlers_ShouldBe_Sealed` | Asserts all query handler implementations are sealed. |
| `QueryHandlers_ShouldResideIn_QueryNamespace` | Asserts all query handlers are located under a `*.Query` namespace. |
| `DomainEventHandlers_ShouldBe_Sealed` | Asserts all domain event handler implementations are sealed. |
| `DomainEventHandlers_ShouldHave_DomainEventHandlerSuffix` | Asserts all domain event handler class names end with `DomainEventHandler`. |
| `Interfaces_InApplicationLayer_ShouldStartWith_I` | Asserts all interfaces in the Application layer are prefixed with `I`. |

#### `Layers/DomainLayerTests.cs` (7 tests)
| Test | Description |
|------|-------------|
| `Entities_ShouldHave_PrivateParameterlessConstructor` | Asserts all domain entities (classes that inherit `AggregateRoot`) have a private parameterless constructor (required by EF Core). |
| `Entities_ShouldNotHave_PublicConstructors` | Asserts no entity exposes a public constructor (creation must go through factory methods). |
| `Entities_ShouldHave_PrivateSetters` | Asserts all public properties on entities have private `set` accessors (encapsulation). |
| `DomainEvents_ShouldBe_Sealed` | Asserts all domain event classes are sealed. |
| `DomainEvents_ShouldHave_DomainEventSuffix` | Asserts all domain events end with `DomainEvent`. |
| `DomainEvents_ShouldResideIn_DomainNamespace` | Asserts all domain events are defined within the `Loop.Domain` namespace hierarchy. |
| `ValueObjects_ShouldBe_Sealed` | Asserts all value objects are sealed. |

#### `Layers/InfrastructureLayerTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `DbContext_ShouldResideIn_InfrastructureLayer` | Asserts the `DbContext` class is located in the Infrastructure project. |
| `DbContext_ShouldBe_Sealed` | Asserts the `DbContext` is sealed. |
| `EntityConfigurations_ShouldBe_Sealed` | Asserts all EF Core entity type configurations are sealed. |
| `EntityConfigurations_ShouldHave_ConfigurationSuffix` | Asserts all entity configuration class names end with `Configuration`. |
| `Repositories_ShouldResideIn_InfrastructureLayer` | Asserts all repository implementations are located in the Infrastructure project. |

#### `Layers/PresentationLayerTests.cs` (7 tests)
| Test | Description |
|------|-------------|
| `Controllers_ShouldHave_ControllerSuffix` | Asserts all controller classes end with `Controller`. |
| `Controllers_ShouldInherit_ControllerBase` | Asserts all controllers inherit from `ControllerBase` (not `Controller`). |
| `Controllers_ShouldResideIn_ControllersNamespace` | Asserts all controllers are located under a `*.Controllers` namespace. |
| `Controllers_ShouldHave_ApiControllerAttribute` | Asserts all controllers are decorated with `[ApiController]`. |
| `Controllers_ShouldHave_RouteAttribute` | Asserts all controllers are decorated with `[Route]`. |
| `PresentationLayer_ShouldNotReference_DomainEntities_Directly` | Asserts the Presentation layer does not directly reference any domain entity class (uses DTOs/requests only). |
| `Controllers_ShouldNotHaveDependencyOn_InfrastructureLayer` | Asserts controllers have no dependency on the Infrastructure layer. |

---

## Integration Tests (`Loop.IntegrationTests`)

#### `ApiIntegrationTests.cs` (5 tests)
| Test | Description |
|------|-------------|
| `SwaggerDocument_ShouldBeServed` | Sends a GET request to `/swagger/v1/swagger.json`. Asserts the response is 200 OK with content type `application/json; charset=utf-8`, confirming Swagger/OpenAPI is properly configured. |
| `ProtectedEndpoint_ShouldReturnUnauthorized_WhenTokenIsMissing` | Sends a GET request to `/api/shops` without an Authorization header. Asserts the response is 401 Unauthorized. |
| `Login_ShouldReturnBadRequest_WhenPayloadIsInvalid` | Sends a POST request to `/api/auth/login` with an empty JSON body `{}`. Asserts the response is 400 Bad Request (validation failure). |
| `GetOfferById_ShouldReturnUnauthorized_WhenTokenIsMissing` | Sends a GET request to `/api/offers/{id}` without an Authorization header. Asserts 401 Unauthorized. |
| `ReceiptsOcr_ShouldReturnBadRequest_WhenFileIsMissing_ForAuthenticatedUser` | Authenticates first, then sends a POST to `/api/receipts/ocr` without a file. Asserts 400 Bad Request. |

---

## Test Files by Location

| # | File | Tests |
|---|------|-------|
| 1 | `Domain/Common/EmailTests.cs` | 3 |
| 2 | `Domain/Common/MoneyTests.cs` | 5 |
| 3 | `Domain/Common/PhoneTests.cs` | 3 |
| 4 | `Domain/Offers/OfferTests.cs` | 5 |
| 5 | `Domain/Receipts/ReceiptTests.cs` | 6 |
| 6 | `Domain/Stamps/StampRedemptionTests.cs` | 1 |
| 7 | `Domain/Stamps/StampTests.cs` | 3 |
| 8 | `Domain/Stamps/StampTransactionTests.cs` | 2 |
| 9 | `Domain/Stamps/UserStampCardTests.cs` | 4 |
| 10 | `Domain/Transactions/EarnTransactionTests.cs` | 2 |
| 11 | `Domain/Transactions/RedeemTransactionTests.cs` | 5 |
| 12 | `Domain/Users/UserTests.cs` | 11 |
| 13 | `Application/Categories/GetCategoriesQueryTests.cs` | 2 |
| 14 | `Application/Offers/GetOfferQueriesTests.cs` | 3 |
| 15 | `Application/Queries/GetUserByIdQueryTests.cs` | 3 |
| 16 | `Application/Receipts/LocalMerchantMatcherTests.cs` | 5 |
| 17 | `Application/Shops/ConfirmPointsRedemptionQrCommandTests.cs` | 11 |
| 18 | `Application/Shops/GetShopByIdQueryTests.cs` | 4 |
| 19 | `Application/Shops/GetShopsQueryTests.cs` | 7 |
| 20 | `Application/Shops/LoginShopAdminCommandTests.cs` | 4 |
| 21 | `Application/Shops/RefreshShopAdminTokenCommandTests.cs` | 4 |
| 22 | `Application/Stamps/GetActiveShopStampsQueryTests.cs` | 2 |
| 23 | `Application/Stamps/GetUserStampCardsQueryTests.cs` | 2 |
| 24 | `Application/Stamps/GenerateStampCollectQrCommandTests.cs` | 3 |
| 25 | `Application/Stamps/ScanStampCollectQrCommandTests.cs` | 9 |
| 26 | `Application/Stamps/GenerateStampRedemptionQrCommandTests.cs` | 3 |
| 27 | `Application/Stamps/ConfirmStampRedemptionQrCommandTests.cs` | 8 |
| 28 | `Application/Users/UserQueryTests.cs` | 6 |
| 29 | `Application/Users/AddPointsToUserCommandTests.cs` | 3 |
| 30 | `Application/Users/LoginUserCommandTests.cs` | 4 |
| 31 | `Application/Users/LogoutUserCommandTests.cs` | 2 |
| 32 | `Application/Users/RegisterUserCommandTests.cs` | 6 |
| 33 | `Application/Users/GeneratePointsRedemptionQrCommandTests.cs` | 4 |
| 34 | `Infrastructure/Ocr/MestalOcrProviderTests.cs` | 13 |
| 35 | `Layers/LayerTests.cs` | 6 |
| 36 | `Layers/ApplicationLayerTests.cs` | 11 |
| 37 | `Layers/DomainLayerTests.cs` | 7 |
| 38 | `Layers/InfrastructureLayerTests.cs` | 5 |
| 39 | `Layers/PresentationLayerTests.cs` | 7 |
| 40 | `Loop.IntegrationTests/ApiIntegrationTests.cs` | 5 |
