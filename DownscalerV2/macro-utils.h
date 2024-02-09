#pragma once

/// Macros ///

/**
 * @brief A macro that defines a getter for a property.
 * @param name The name of the property.
 */
#define GETTER(name) \
  auto Get##name() const { return this->name; }

/**
 * @brief A macro that defines a setter for a property.
 * @param name The name of the property.
 */
#define SETTER(name) \
  void Set##name(const decltype(name)& value) { this->name = value; }

/**
 * @brief A macro that defines a getter and setter for a property.
 * @param name The name of the property.
 */
#define ACCESSOR(name) \
  GETTER(name) \
  SETTER(name)

/**
 * @brief A macro that defines a getter and setter for a property. Unlike ACCESSOR, this macro is used for properties that
 *        need to notify when the property has changed.
 * @param PropertyName The name of the property.
 */
#define PROPERTY(propertyName) \
  auto Get##propertyName() const { return propertyName; } \
  void Set##propertyName(const decltype(propertyName) value) { \
    if (propertyName != value) { \
      propertyName = value; \
      NotifyPropertyChanged(L#propertyName); \
    } \
  }

/**
 * @brief A macro that defines a getter and setter for a property. Unlike ACCESSOR, this macro is used for properties that
 *        need to notify when the property has changed. This macro is used for reference types.
 * @param PropertyName The name of the property.
 */
#define REF_PROPERTY(propertyName) \
  auto Get##propertyName() const { return propertyName; } \
  void Set##propertyName(const decltype(propertyName)& value) { \
     if (propertyName != value) { \
          propertyName = value; \
          NotifyPropertyChanged(L#propertyName); \
      } \
  }

/**
 * @brief A macro used to retrieve the value of a property.
 * @param name The name of the property.
 */
#define GET(name) Get##name()

/**
 * @brief A macro used to set the value of a property.
 * @param name The name of the property.
 * @param value The value to set the property to.
 */
#define SET(name, value) Set##name(value)
