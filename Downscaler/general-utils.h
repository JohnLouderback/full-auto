#pragma once
#include <iostream>
#include <string>

/**
 * @brief Output an error to stderr and exit the program with a failure status.
 * @param message The error message to output.
 */
inline void FatalError(std::string message) {
  std::cerr << "Error: " << message << '\n';
  exit(EXIT_FAILURE);
}

inline void FatalError(const std::ostringstream& stream) {
  std::cerr << "Error: " << stream.str() << '\n';
  exit(EXIT_FAILURE);
}
