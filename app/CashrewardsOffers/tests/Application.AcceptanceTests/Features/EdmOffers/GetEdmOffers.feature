Feature: GetEdmOffers

An API that gets offer information in a format to be consumed by Electronic Direct Mail provider LeanPlum.

Scenario: Get offers
	Given offers exist in the ShopGo database
	| Title   | Ranking |
	| Offer-1 | 1       |
	| Offer-2 | 99      |
	| Offer-3 | 3       |
	Given the offer sync job has run
	When I send an EDM offer query
	Then I should receive EDM offers in the correct order
	| Title   |
	| Offer-2 |
	| Offer-3 |
	| Offer-1 |


	
Scenario: Get offers with terms
	Given offers exist in the ShopGo database
	| MerchantId | Title   | Ranking |
	| 100        | Offer-1 | 5       |
	| 100        | Offer-2 | 4       |
	| 101        | Offer-3 | 3       |
	Given merchant tiers exist in the ShopGo database
	| MerchantId | terms                        |
	| 100        | Capped at $10 - Ends 11:59pm |
	| 101        | Capped at $20 - Ends soon    |
	Given the offer sync job has run
	When I send an EDM offer query
	Then I should receive EDM offers in the correct order
	| Title     | Terms                        |
	| Offer-1 | Capped at $10 - Ends 11:59pm |
	| Offer-2 | Capped at $10 - Ends 11:59pm |
	| Offer-3 | Capped at $20 - Ends soon    |



Scenario: Order offers by ranking then end date
	Given offers exist in the ShopGo database
	| MerchantId | Title   | Ranking | EndDateTime           |
	| 100        | Offer-1 | 5       |                       |
	| 100        | Offer-2 | 4       |                       |
	| 101        | Offer-3 | 1       | 3/01/2022 11:59:59 PM |
	| 101        | Offer-4 | 1       | 2/01/2022 11:59:59 PM |
	| 101        | Offer-5 | 1       | 1/01/2022 11:59:59 PM |
	Given the offer sync job has run
	When I send an EDM offer query
	Then I should receive EDM offers in the correct order
	| Title   |
	| Offer-1 |
	| Offer-2 |
	| Offer-5 |
	| Offer-4 |
	| Offer-3 |



Scenario: Order offers by experiment 1 - top 2 favourites by best offer rate
	Given Unleash feature toggles
	| FeatureToggleName |
	| Exp1              |
	| EnrolExp1         |
	Given offers exist in the ShopGo database
	| MerchantId | Title    | Ranking | OfferString |
	| 1          | Offer-1  | 10      | up to 20%   |
	| 2          | Offer-2  | 9       | up to $25   |
	| 3          | Offer-3  | 8       | 10%         |
	| 4          | Offer-4  | 7       | 12%         |
	| 5          | Offer-5  | 6       | 14%         |
	| 6          | Offer-6  | 5       | up to 25%   |
	| 7          | Offer-7  | 4       | $50         |
	| 8          | Offer-8  | 3       | 50%         |
	| 9          | Offer-9  | 2       | up to 50%   |
	| 10         | Offer-10 | 1       | up to $50   |
	Given person with CognitoId '100' and NewMemberId '300'
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString |
	| 100       | 3          | merch-3          |
	| 100       | 4          | merch-4          |
	| 100       | 5          | merch-5          |
	| 100       | 6          | merch-6          |
	| 100       | 7          | merch-7          |
	| 100       | 8          | merch-8          |
	| 100       | 9          | merch-9          |
	| 100       | 10         | merch-10         |
	Given person with CognitoId '100' is enroled in experiment 'Exp1'
	Given the offer sync job has run
	When I send an EDM offer query with NewMemberId '300'
	Then I should receive EDM offers in the correct order
	| Title    |
	| Offer-8  |
	| Offer-9  |
	| Offer-1  |
	| Offer-2  |
	| Offer-3  |
	| Offer-4  |
	| Offer-5  |
	| Offer-6  |
	| Offer-7  |
	| Offer-10 |
	


Scenario: Order offers by experiment 2 - top 2 favourites by soonest offer ending date
	Given Unleash feature toggles
	| FeatureToggleName |
	| Exp2              |
	| EnrolExp2         |
	Given offers exist in the ShopGo database
	| MerchantId | Title   | Ranking | EndDateTime           |
	| 1          | Offer-1 | 10      | 7/01/2022 11:59:59 PM |
	| 2          | Offer-2 | 9       | 6/01/2022 11:59:59 PM |
	| 3          | Offer-3 | 8       | 5/01/2022 11:59:59 PM |
	| 4          | Offer-4 | 7       | 4/01/2022 11:59:59 PM |
	| 5          | Offer-5 | 6       | 3/01/2022 11:59:59 PM |
	| 6          | Offer-6 | 5       | 2/01/2022 11:59:59 PM |
	| 7          | Offer-7 | 4       | 1/01/2022 11:59:59 PM |
	Given person with CognitoId '100' and NewMemberId '300'
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString |
	| 100       | 3          | merch-3          |
	| 100       | 4          | merch-4          |
	| 100       | 5          | merch-5          |
	| 100       | 6          | merch-6          |
	Given person with CognitoId '100' is enroled in experiment 'Exp2'
	Given the offer sync job has run
	When I send an EDM offer query with NewMemberId '300'
	Then I should receive EDM offers in the correct order
	| Title    |
	| Offer-6  |
	| Offer-5  |
	| Offer-1  |
	| Offer-2  |
	| Offer-3  |
	| Offer-4  |
	| Offer-7  |
	


Scenario: Order offers by experiment 3 - top 2 favourites by selection order
	Given Unleash feature toggles
	| FeatureToggleName |
	| Exp3              |
	| EnrolExp3         |
	Given offers exist in the ShopGo database
	| MerchantId | Title    | Ranking |
	| 1          | Offer-1  | 10      |
	| 2          | Offer-2  | 9       |
	| 3          | Offer-3  | 8       |
	| 4          | Offer-4  | 7       |
	| 5          | Offer-5  | 6       |
	| 6          | Offer-6  | 5       |
	| 7          | Offer-7  | 4       |
	Given person with CognitoId '100' and NewMemberId '300'
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString | SelectionOrder |
	| 100       | 3          | merch-3          | 3              |
	| 100       | 4          | merch-4          | 0              |
	| 100       | 5          | merch-5          | 2              |
	| 100       | 6          | merch-6          | 1              |
	Given person with CognitoId '100' is enroled in experiment 'Exp3'
	Given the offer sync job has run
	When I send an EDM offer query with NewMemberId '300'
	Then I should receive EDM offers in the correct order
	| Title    |
	| Offer-4  |
	| Offer-6  |
	| Offer-1  |
	| Offer-2  |
	| Offer-3  |
	| Offer-5  |
	| Offer-7  |
	


Scenario: Exclude offers already contained in EDM campigns
	Given offers exist in the ShopGo database
	| OfferId | Title     | Ranking |
	| 201     | Offer-201 | 1       |
	| 202     | Offer-202 | 2       |
	| 203     | Offer-203 | 3       |
	| 204     | Offer-204 | 4       |
	| 205     | Offer-205 | 5       |
	Given Offer with OfferId '203' is in EDM campaign with CampaignId '101'
	Given the offer sync job has run
	When I send an EDM offer query with EDM campaignId '101'
	Then I should receive EDM offers in the correct order
	| Title     |
	| Offer-205 |
	| Offer-204 |
	| Offer-202 |
	| Offer-201 |
