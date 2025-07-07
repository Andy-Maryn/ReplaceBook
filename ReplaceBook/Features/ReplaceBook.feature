Feature: Replace a book in the user's list

  Scenario: Replace book for a user
    Given a user exists in the Bookstore
    And I add a book to the user's library
    When I replace the book with a different book
    Then the user's library contains only the new book