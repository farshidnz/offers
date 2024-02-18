Feature: ANZ Offers API - ID

    As PM
    I want to create an API for ANZ to show the ID for a list of merchants and offers from the following carousels:
    Max+ANZ offers carousel
    Popular Stores carousel
    Featured Offers carousel
    In-Store carousel

    So that ANZ can get a list of updated merchants and offers to be stored in their system to be displayed on the ANZ app for users to view CR offers and merchants and be redirected to the CR site

    Scenario Outline: Available fields in API response for ANZ

        Given API for ANZ is available
        When I call the api
        And auth token is valid
        Then the API returns the '<fields>' for Merchant and Offer in the response as '<mandatory>'

        Examples:
            | fields              | mandatory |
            | TotalOffersCount    | mandatory |
            | Items[].Id          | mandatory |
            | Items[].Merchant.Id | mandatory |
            | Items[].Offer.Id    | optional  |



    Scenario Outline: List of Offers in Featured Offers Carousel

        Given API for ANZ is available
        And Offer is set to currently featured '<currently featured>'
        And Offer is mapped to '<client mapping>'
        When I call the api
        And auth token is valid
        Then the API '<result>' the offer in the response with the specified fields

        Examples:
            | currently featured | client mapping | result  |
            | Yes                | ANZ            | exclude |
            | Yes                | ANZ, CR        | include |
            | Yes                | ANZ, CR, MME   | include |
            | Yes                | CR             | include |
            | Yes                | CR, MME        | include |
            | Yes                | MME            | exclude |
            | NO                 | ANZ            | exclude |
            | No                 | ANZ, CR        | exclude |
            | No                 | ANZ, CR, MME   | exclude |
            | No                 | CR             | exclude |
            | No                 | CR, MME        | exclude |
            | No                 | MME            | exclude |


    Scenario Outline: List of Merchants in Popular Stores Carousel

        Given API for ANZ is available
        And Merchant is set to '<Popular>'
        And Merchant is mapped to '<client mapping>'
        When I call the api
        And auth token is valid
        Then the API '<result>' the Merchant in the response with the specified fields

        Examples:
            | Popular | client mapping | result  |
            | Yes     | ANZ            | exclude |
            | Yes     | ANZ, CR        | include |
            | Yes     | ANZ, CR, MME   | include |
            | Yes     | CR             | include |
            | Yes     | CR, MME        | include |
            | Yes     | MME            | exclude |
            | NO      | ANZ            | exclude |
            | No      | ANZ, CR        | exclude |
            | No      | ANZ, CR, MME   | exclude |
            | No      | CR             | exclude |
            | No      | CR, MME        | exclude |
            | No      | MME            | exclude |


    Scenario Outline: List of Merchants in In-Store Carousel

        Given API for ANZ is available
        And Merchants network is set to '<NetworkName>'
        And Merchant is mapped to '<client mapping>'
        When I call the api
        And auth token is valid
        Then the API '<result>' the Merchant in the response with the specified fields

        Examples:
            | NetworkName     | client mapping | result  |
            | InStore         | ANZ            | exclude |
            | InStore         | ANZ, CR        | include |
            | InStore         | ANZ, CR, MME   | include |
            | InStore         | CR             | include |
            | InStore         | CR, MME        | include |
            | InStore         | MME            | exclude |
            | Other network   | ANZ            | exclude |
            | Other network   | ANZ, CR        | exclude |
            | Other network   | ANZ, CR, MME   | exclude |
            | Other network   | CR             | exclude |
            | Other network   | CR, MME        | exclude |
            | NOther networko | MME            | exclude |


    Scenario Outline: Display ANZ merchant and offers enabled for Mobile

        Given '<API>' for ANZ is available
        And Merchants Mobile Enabled is set to '<MobileEnabled>'
        And Merchants Tablet Enabled is set to '<MobileAppEnabled>'
        And Merchants Mobile App Enabled is set to '<MobileAppEnabled>'
        When I call the api
        And auth token is valid
        Then the API '<result>' the Merchant and Offer in the response with the specified fields

        Examples:
            | API             | MobileEnabled | TabletEnabled | MobileAppEnabled | result  |
            | Featured offers | Yes           | Yes           | Yes              | include |
            | Featured offers | Yes           | Yes           | No               | include |
            | Featured offers | Yes           | No            | Yes              | include |
            | Featured offers | Yes           | No            | No               | include |
            | Featured offers | No            | Yes           | Yes              | exclude |
            | Featured offers | No            | Yes           | No               | exclude |
            | Featured offers | No            | No            | Yes              | exclude |
            | Featured offers | No            | No            | No               | exclude |
            | In-Store        | Yes           | Yes           | Yes              | include |
            | In-Store        | Yes           | Yes           | No               | include |
            | In-Store        | Yes           | No            | Yes              | include |
            | In-Store        | Yes           | No            | No               | include |
            | In-Store        | No            | Yes           | Yes              | exclude |
            | In-Store        | No            | Yes           | No               | exclude |
            | In-Store        | No            | No            | Yes              | exclude |
            | In-Store        | No            | No            | No               | exclude |
            | Popular Store   | Yes           | Yes           | Yes              | include |
            | Popular Store   | Yes           | Yes           | No               | include |
            | Popular Store   | Yes           | No            | Yes              | include |
            | Popular Store   | Yes           | No            | No               | include |
            | Popular Store   | No            | Yes           | Yes              | exclude |
            | Popular Store   | No            | Yes           | No               | exclude |
            | Popular Store   | No            | No            | Yes              | exclude |
            | Popular Store   | No            | No            | No               | exclude |


    Scenario Outline: Suppressed merchants not displayed in any carousels of ANZ app

        Given '<API>' for ANZ is available
        And Merchants Suppressed for Max member is set to '<IsPremiumDisabled>'
        When I call the api
        And auth token is valid
        Then the API '<result>' the Merchant and Offer in the response with the specified fields

        Examples:
            | API             | IsPremiumDisabled | result  |
            | Featured offers | 0                 | include |
            | Featured offers | 1                 | exclude |
            | In-Store        | 0                 | include |
            | In-Store        | 1                 | exclude |
            | Popular Store   | 0                 | include |
            | Popular Store   | 1                 | exclude |
