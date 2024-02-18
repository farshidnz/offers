Feature: GetEdmMerchants

An API that gets merchant information in a format to be consumed by Electronic Direct Mail provider LeanPlum.

Scenario: Default merchants without any personalisation
	Given default merchants exist in the ShopGo database
	Given the merchant sync job has run
	When I send an EDM merchant query
	Then I should receive EDM merchants in the correct order
	| MerchantHyphenatedString |
	| amazon-australia         |
	| the-iconic               |
	| myer                     |
	| david-jones              |
	| bonds                    |
	| groupon                  |



Scenario: Exclude unavailable merchants
	Given default merchants exist in the ShopGo database
	Given the merchant 'amazon-australia' is unavailable
	Given the merchant sync job has run
	When I send an EDM merchant query
	Then I should receive EDM merchants in the correct order
	| MerchantHyphenatedString |
	| the-iconic               |
	| myer                     |
	| david-jones              |
	| bonds                    |
	| groupon                  |



Scenario: Include persons favourites first
	Given default merchants exist in the ShopGo database
	Given person with CognitoId '100' and NewMemberId '300'
	Given merchants exist in the ShopGo database
	| MerchantId | HyphenatedString |
	| 1003604    | apple            |
	| 1004971    | big-w            |
	| 1003511    | veronika-maine   |
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString | SelectionOrder |
	| 100       | 1003604    | apple            | 0              |
	| 100       | 1004971    | big-w            | 1              |
	Given the merchant sync job has run
	When I send an EDM merchant query with NewMemberId '300'
	Then I should receive EDM merchants in the correct order
	| MerchantHyphenatedString |
	| apple                    |
	| big-w                    |
	| amazon-australia         |
	| the-iconic               |
	| myer                     |
	| david-jones              |



Scenario: Include all persons favourites without any default merchants
	Given default merchants exist in the ShopGo database
	Given person with CognitoId '100' and NewMemberId '300'
	Given merchants exist in the ShopGo database
	| MerchantId | HyphenatedString |
	| 1003604    | apple            |
	| 1004971    | big-w            |
	| 1001460    | get-wines-direct |
	| 1003511    | veronika-maine   |
	| 1003663    | liquorland       |
	| 1003877    | dell-australia   |
	| 1002933    | hotels-com       |
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString | SelectionOrder |
	| 100       | 1004971    | big-w            | 1              |
	| 100       | 1001460    | get-wines-direct | 2              |
	| 100       | 1003511    | veronika-maine   | 3              |
	| 100       | 1003663    | liquorland       | 4              |
	| 100       | 1003877    | dell-australia   | 5              |
	| 100       | 1002933    | hotels-com       | 6              |
	Given the merchant sync job has run
	When I send an EDM merchant query with NewMemberId '300'
	Then I should receive EDM merchants in the correct order
	| MerchantHyphenatedString |
	| big-w                    |
	| get-wines-direct         |
	| veronika-maine           |
	| liquorland               |
	| dell-australia           |
	| hotels-com               |

	

Scenario: When a person selects a default merchant as favourite then it appears first
	Given default merchants exist in the ShopGo database
	Given person with CognitoId '100' and NewMemberId '300'
	Given person has selected favourites
	| CognitoId | MerchantId | hyphenatedString | SelectionOrder |
	| 100       | 1001527    | bonds            | 0              |
	Given the merchant sync job has run
	When I send an EDM merchant query with NewMemberId '300'
	Then I should receive EDM merchants in the correct order
	| MerchantHyphenatedString |
	| bonds                    |
	| amazon-australia         |
	| the-iconic               |
	| myer                     |
	| david-jones              |
	| groupon                  |

